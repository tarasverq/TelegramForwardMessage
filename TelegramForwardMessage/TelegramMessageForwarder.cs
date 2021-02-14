

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TelegramForwardMessage.Exceptions;
using TeleSharp.TL;
using TeleSharp.TL.Account;
using TeleSharp.TL.Channels;
using TeleSharp.TL.Contacts;
using TeleSharp.TL.Messages;
using TLSharp.Core;
using TLSharp.Core.Network.Exceptions;
using TLSharp.Core.Utils;
using TLChatFull = TeleSharp.TL.TLChatFull;

namespace TelegramForwardMessage
{
    public interface ITelegramMessageForwarder
    {
        Task<bool> Initialize();
        Task SendCode(string code);
        Task ReplyInDiscussion(string channelUsername, string message);
    }

    public class TelegramMessageForwarder : ITelegramMessageForwarder
    {
        private bool _authorized;
        private readonly string _phone;
        private readonly Tuple<int, int> _delay;
        private readonly TelegramClient _client;

        private string _hash;

        public TelegramMessageForwarder(int clientId, string clientHash, string phone, Tuple<int, int> delay)
        { 
            _phone = phone;
            _delay = delay;

            _client = new TelegramClient(clientId, clientHash, null, phone);

        }

        /// <summary>
        /// Проверяет авторизацию и отправляет код для логина при необходимости
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Initialize()
        {
            await _client.ConnectAsync();

            if (!_authorized && !_client.IsUserAuthorized())
            {
                _hash = await _client.SendCodeRequestAsync("+" + _phone);
                return true;
            }

            _authorized = true;
            return false;
        }

        /// <summary>
        /// Отправляет код обратно в телегу
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task SendCode(string code)
        {
            if (!_authorized)
            {
                TLUser auth = await _client.MakeAuthAsync(_phone, _hash, code);
                _authorized = true;
            }
        }


        

        /// <summary>
        /// Получить последнее сообщение из канала
        /// </summary>
        /// <param name="channelFrom"></param>
        /// <returns></returns>
        private async Task<TLMessage> GetLastMessage(TLChannel channelFrom)
        {
            string type = typeof(TLRequestGetHistory).ToString();
            if (!_methodCounter.TryAdd(type, 1))
                _methodCounter[type]++;
            
            TLAbsMessages resp = await _client.GetHistoryAsync(new TLInputPeerChannel()
            {
                ChannelId = channelFrom.Id,
                AccessHash = channelFrom.AccessHash ?? 0,
            }, limit: 100);

            List<TLMessage> messages = ((TLChannelMessages) resp).Messages?
                .Where(x => x is TLMessage message && message.ReplyMarkup == null)
                .Cast<TLMessage>()
                .ToList();

            TLMessage lastMessage = messages?.First();
            if (lastMessage?.GroupedId != null)
            {
                lastMessage = messages.Where(x => x.GroupedId == lastMessage?.GroupedId).Last();
            }
            
            return lastMessage;
        }

        public async Task ReplyInDiscussion(string channelUsername, string message)
        {
            Tuple<TLChannel, TLChannelFull, TLChannel> channelInfo = await GetFullChannel(channelUsername);
            TLChannel channel = channelInfo.Item1;
            TLChannel chat = channelInfo.Item3;
            TLChannelFull fullInfo = channelInfo.Item2;
            if (chat == null)
            {
                throw new NoLinkedChatException();
            }
            else
            {
                await ReplyInDiscussion(channel, message, chat);
            }
        }

        private async Task ReplyInDiscussion(TLChannel channel, string message, TLChannel chat)
        {
            TLMessage lastMessage = await GetLastMessage(channel);
            await JoinChannel(channel);

            TLChannel targetChannel = null;
            int targetPostId = 0;
            try
            {
                DiscussionInfo discussionInfo = await GetDiscussionInfo(channel, lastMessage);
                targetChannel = new TLChannel()
                {
                    Id = discussionInfo.Id,
                    AccessHash = discussionInfo.AccessHash
                };
                targetPostId = discussionInfo.PostId;
            }
            catch (NoDiscussionsException e)
            {
                Tuple<TLChannel, TLChannelFull, TLChannel> chatInfo = await GetFullChannel(new TLInputChannel() {ChannelId = chat.Id, AccessHash = chat.AccessHash ?? 0});
                targetChannel = new TLChannel()
                {
                    Id = chatInfo.Item1.Id,
                    AccessHash = chatInfo.Item1.AccessHash
                };
                targetPostId = chatInfo.Item2.PinnedMsgId ?? 0;
            }

            await JoinChannel(targetChannel);

            try
            {
                await ReplyTo(targetChannel, message, targetPostId);
            }
            catch (InvalidOperationException e)
                when (e.Message == "MSG_ID_INVALID")
            {
                RandomHelper.SuperMario();
                throw;
            }
            // catch (InvalidOperationException e)
            //     when (e.Message == "CHAT_WRITE_FORBIDDEN")
            // {
            //     throw new NoLinkedChatException(e.Message);
            // }
        }

        private async Task<DiscussionInfo> GetDiscussionInfo(TLChannel channel, TLMessage lastMessage)
        {
            TLRequestGetDiscussionMessage discussionMessageRequest = new TLRequestGetDiscussionMessage()
            {
                MsgId = lastMessage.Id,
                Peer = new TLInputPeerChannel()
                {
                    ChannelId = channel.Id,
                    AccessHash = channel.AccessHash ?? 0,
                },
            };
            TLDiscussionMessage discussionMessageResponse;
            try
            {
                discussionMessageResponse = await SendRequestAsync<TLDiscussionMessage>(discussionMessageRequest);
            }
            catch (InvalidOperationException e) when (e.Message == "MSG_ID_INVALID")
            {
                throw new NoDiscussionsException(e.Message, e)
                {
                    Username = channel.Username,
                    ChatId = channel.Id,
                    MessageId = lastMessage.Id,
                    MessageText = lastMessage.Message ?? String.Empty,
                };
            }

            TLMessage discussionMessage = discussionMessageResponse.Messages.Where(x => x is TLMessage)
                .Cast<TLMessage>().FirstOrDefault();
            if (discussionMessage == null)
            {
                throw new NoDiscussionsException()
                {
                    Username = channel.Username,
                    ChatId = channel.Id,
                    MessageId = lastMessage.Id,
                    MessageText = lastMessage.Message ?? String.Empty,
                };
            }
            
            int discussionChatId = ((TLPeerChannel) discussionMessage.PeerId).ChannelId;
            
            TLChannel discussionsChannel =
                discussionMessageResponse.Chats
                    .Where(x => x is TLChannel possibleChannel && possibleChannel.Id == discussionChatId)
                    .Cast<TLChannel>()
                    .FirstOrDefault();
            if (discussionsChannel == null)
                throw new NoDiscussionsException()
                {
                    Username = channel.Username,
                    ChatId = channel.Id,
                    MessageId = lastMessage.Id,
                    MessageText = lastMessage.Message ?? String.Empty,
                };
            
            return new DiscussionInfo()
            {
                Id = discussionsChannel.Id,
                AccessHash = discussionsChannel.AccessHash,
                PostId = discussionMessage.Id
            };
        }

        /// <summary>
        /// Получить инфу о канале
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public async Task<TLChannel> GetChannel(string username)
        {
            if (!_authorized)
                throw new UnauthorizedAccessException("Not authorized");

            if (username.StartsWith("AAAAA"))
            {
               return await GetPrivateChannel(username);
            }
            
            TLRequestResolveUsername requestResolveUsername = new TLRequestResolveUsername()
            {
                Username = username
            };

            TLResolvedPeer peer;
            try
            {
                peer = await SendRequestAsync<TLResolvedPeer>(requestResolveUsername);
            }
            catch (InvalidOperationException invalidOperationException)
                when(invalidOperationException.Message == "USERNAME_NOT_OCCUPIED"
                     || invalidOperationException.Message == "USERNAME_INVALID")
            {
                throw new ChannelNotFoundException(invalidOperationException.Message, invalidOperationException) {ChannelName = username};
            }
               

            TLChannel channel = peer.Chats.FirstOrDefault(x=> x is TLChannel channelFilter && channelFilter.Username == username) as TLChannel;
            if (channel == null)
                throw new ChannelNotFoundException() {ChannelName = username};
            
            return channel;
        }

        private async Task<TLChannel> GetPrivateChannel(string hash)
        {
            TLRequestCheckChatInvite requestCheckChatInvite = new TLRequestCheckChatInvite()
            {
                Hash = hash
            };
            TLAbsChatInvite inviteInfo;
            try
            {
                inviteInfo = await SendRequestAsync<TLAbsChatInvite>(requestCheckChatInvite);
            }
            catch (InvalidOperationException invalidOperationException)
                when (invalidOperationException.Message == "INVITE_HASH_EXPIRED"
                      || invalidOperationException.Message == "INVITE_HASH_INVALID")
            {
                throw new ChannelNotFoundException(invalidOperationException.Message, invalidOperationException)
                    {ChannelName = hash};
            }

            if (inviteInfo is TLChatInvitePeek peek)
            {
                TLChannel channel = peek.Chat as TLChannel;
                if (channel == null)
                    throw new ChannelNotFoundException() {ChannelName = hash};
                return channel;
            }
            else if (inviteInfo is TLChatInviteAlready already)
            {
                TLChannel channel = already.Chat as TLChannel;
                if (channel == null)
                    throw new ChannelNotFoundException() {ChannelName = hash};
                return channel;
            }
            else if (inviteInfo is TLChatInvite invite)
            {
                TLRequestImportChatInvite importChatInvite = new TLRequestImportChatInvite()
                {
                    Hash = hash,
                };
                var resp = await SendRequestAsync<TLUpdates>(importChatInvite);

                var channel =
                    resp.Chats.First(x => x is TLChannel channelFilter/* && channelFilter.Title == invite.Title*/) as TLChannel;
                return channel;
                //TODO FIX
            }
            else
            {
                throw new NotImplementedException($"Unknown type of TLAbsChatInvite. hash '{hash}'");
            }
        }

        public async Task<Tuple<TLChannel, TLChannelFull, TLChannel>> GetFullChannel(string username)
        {
            if (!_authorized)
                throw new UnauthorizedAccessException("Not authorized");

            TLChannel channel = await GetChannel(username);
            return await GetFullChannel(new TLInputChannel()
            {
                ChannelId = channel.Id,
                AccessHash = channel.AccessHash ?? 0
            });

        }

        private async Task<Tuple<TLChannel, TLChannelFull, TLChannel>> GetFullChannel(TLInputChannel inputChannel)
        {
            TLRequestGetFullChannel requestGetFullChannel = new TLRequestGetFullChannel()
            {
                Channel = inputChannel
            };

            TeleSharp.TL.Messages.TLChatFull chatFull = await SendRequestAsync<TeleSharp.TL.Messages.TLChatFull>(requestGetFullChannel);

            
            TLChannelFull channelFull = chatFull.FullChat as TLChannelFull;
            TLChannel channel =  chatFull.Chats.First(x =>
                x is TLChannel channelFilter && channelFilter.Id == channelFull?.Id) as TLChannel;
            TLChannel chat = chatFull.Chats.FirstOrDefault(x =>
                x is TLChannel channelFilter && channelFilter.Id == channelFull?.LinkedChatId) as TLChannel;
            
            return new Tuple<TLChannel, TLChannelFull, TLChannel>(channel, channelFull, chat);
        }

        public async Task SetOnline()
        {
            TLRequestUpdateStatus requestUpdateStatus = new TLRequestUpdateStatus()
            {
                Offline = false,
            };
            bool ok = await SendRequestAsync<bool>(requestUpdateStatus);
        }

        public Task ReplyTo(TLChannel channelTo, string message, int? replyMsgId = null)
        {
            TLAbsInputPeer to = new TLInputPeerChannel()
            {
                ChannelId = channelTo.Id,
                AccessHash = channelTo.AccessHash ?? 0
            };
            return ReplyTo(to, message, replyMsgId);
        }

      

        private async Task ReplyTo(TLAbsInputPeer to, string message, int? replyMsgId = null)
        {
            TLRequestSendMessage req = new TLRequestSendMessage()
            {
                Peer = to,
                Message = message,
                RandomId = Helpers.GenerateRandomLong(),
                ReplyToMsgId = replyMsgId
            };
            var response = await SendRequestAsync<TLUpdates>(req);
        } 
        
        private async Task JoinChannel(TLChannel channelTo)
        {
            TLRequestJoinChannel req = new TLRequestJoinChannel()
            {
                Channel = new TLInputChannel()
                {
                    ChannelId = channelTo.Id,
                    AccessHash = channelTo.AccessHash ?? 0
                },
            };
            await SendRequestAsync<TLUpdates>(req);
        }

        public async Task<T> SendRequestAsync<T>(TLMethod methodToExecute,
            CancellationToken token = default(CancellationToken))
        {
            string type = methodToExecute.GetType().ToString();
            if (!_methodCounter.TryAdd(type, 1))
                _methodCounter[type]++;
            try
            {
                await Task.Delay(RandomHelper.GetRandomDelay(_delay.Item1, _delay.Item2));
                return await _client.SendRequestAsync<T>(methodToExecute, token);
            }
            catch (FloodException e)
            {
                throw new FloodException(e.TimeToWait, 
                    System.Text.Json.JsonSerializer.Serialize(_methodCounter, options: 
                    new System.Text.Json.JsonSerializerOptions(){WriteIndented = true}), e);
            }
           
        }

        private Dictionary<string, int> _methodCounter = new Dictionary<string, int>();
        public IReadOnlyDictionary<string, int> MethodCounter => _methodCounter;

        public void ClearMethodCounter()
        {
            _methodCounter = new();
        }
    }

    class DiscussionInfo
    {
        public int Id { get; set; }
        public long? AccessHash { get; set; }
        public int PostId { get; set; }
    }

    class Trash
    {  
        // public Task ReplyTo(TLChat chatTo, string message, int? replyMsgId = null)
        //      {
        //          TLAbsInputPeer to = new TLInputPeerChat()
        //          {
        //              ChatId = chatTo.Id
        //          };
        //          return ReplyTo(to, message, replyMsgId);
        //      }
           // /// <summary>
        // /// Получить последнее сообщение из канала
        // /// </summary>
        // /// <param name="channelFrom"></param>
        // /// <returns></returns>
        // public async Task<TLMessage> GetPinnedMessage(TLChannel channel)
        // {
        //     TLChannelMessages resp = (TLChannelMessages) await _client.GetHistoryAsync(new TLInputPeerChannel()
        //     {
        //         ChannelId = channel.Id,
        //         AccessHash = channel.AccessHash ?? 0,
        //     }, limit: 100);
        //
        //     TLMessage lastMessage = (TLMessage) resp.Messages?.Where(x => x is TLMessage message).FirstOrDefault();
        //
        //     if (lastMessage == null)
        //     {
        //         int maximum = resp.Count > 5000 ? 5000 : resp.Count;
        //         int current = 100;
        //         do
        //         {
        //             resp = (TLChannelMessages) await _client.GetHistoryAsync(new TLInputPeerChannel()
        //             {
        //                 ChannelId = channel.Id,
        //                 AccessHash = channel.AccessHash ?? 0,
        //             }, limit: 100, addOffset: current);
        //             
        //             
        //             lastMessage = (TLMessage) resp.Messages?.Where(x => x is TLMessage message && message.Pinned).FirstOrDefault();
        //             if(lastMessage != null)
        //                 break;
        //             current += 100;
        //         } while (current >= maximum);
        //     }
        //     return lastMessage;
        // }     
        
        // private async Task<TLMessage> GetLastForwardMessage(TLChannel channelFrom, int channelId, int postId)
        // {
        //     TLChannelMessages resp = (TLChannelMessages) await _client.GetHistoryAsync(new TLInputPeerChannel()
        //     {
        //         ChannelId = channelFrom.Id,
        //         AccessHash = channelFrom.AccessHash ?? 0,
        //     }, limit: 100);
        //
        //     TLMessage lastMessage = (TLMessage) resp.Messages?.Where(x =>
        //         x is TLMessage message
        //         && ((TLPeerChannel)message.FwdFrom?.FromId)?.ChannelId == channelId
        //         && message.FwdFrom?.ChannelPost == postId
        //     ).First();
        //     return lastMessage;
        // }
        
        // /// <summary>
        // /// Пересылает последнее сообщение из канала в канал
        // /// </summary>
        // /// <param name="channelFrom"></param>
        // /// <param name="channelTo"></param>
        // /// <returns></returns>
        // /// <exception cref="UnauthorizedAccessException"></exception>
        // public Task<int> Forward(TLChannel channelFrom, TLChannel channelTo)
        // {
        //     if (!_authorized)
        //         throw new UnauthorizedAccessException("Not authorized");
        //
        //
        //     TLAbsInputPeer to = new TLInputPeerChannel()
        //     {
        //         ChannelId = channelTo.Id,
        //         AccessHash = channelTo.AccessHash ?? 0
        //     };
        //
        //     return ForwardLastMessage(channelFrom, to);
        // }
        //
        // public Task<int> Forward(TLChannel channelFrom, TLChat chatTo)
        // {
        //     if (!_authorized)
        //         throw new UnauthorizedAccessException("Not authorized");
        //
        //
        //     TLAbsInputPeer to = new TLInputPeerChat()
        //     {
        //         ChatId = chatTo.Id,
        //     };
        //
        //     return ForwardLastMessage(channelFrom, to);
        // }
        //
        // public async Task<int> ForwardLastMessage(TLChannel channelFrom, TLAbsInputPeer to)
        // {
        //     TLAbsInputPeer from = new TLInputPeerChannel()
        //     {
        //         ChannelId = channelFrom.Id,
        //         AccessHash = channelFrom.AccessHash ?? 0
        //     };
        //     TLMessage lastMessage = await GetLastMessage(channelFrom);
        //
        //     return await ForwardMessage(to, @from, lastMessage.Id);
        // }
        //
        // private async Task<int> ForwardMessage(TLAbsInputPeer to, TLAbsInputPeer @from, int lastMessageId)
        // {
        //     TLRequestForwardMessages requestForwardMessages = new TLRequestForwardMessages()
        //     {
        //         FromPeer = @from,
        //         ToPeer = to,
        //         Id = new TLVector<int>() {lastMessageId},
        //         RandomId = new TLVector<long>() {Helpers.GenerateRandomLong()},
        //     };
        //
        //     try
        //     {
        //         TLUpdates forwardResult = await _client.SendRequestAsync<TLUpdates>(requestForwardMessages);
        //         int messageId = ((TLUpdateMessageID) forwardResult.Updates.First(x => x is TLUpdateMessageID)).Id;
        //         return messageId;
        //     }
        //     catch (Exception e)
        //     {
        //         //TODO залоггировать ошибку
        //         throw;
        //     }
        // }
    }
}
