

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using TeleSharp.TL;
using TeleSharp.TL.Channels;
using TeleSharp.TL.Contacts;
using TeleSharp.TL.Messages;
using TLSharp.Core;
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
            Tuple<TLChannel, TLChannelFull> channelInfo = await GetFullChannel(channelUsername);
            TLChannel channel = channelInfo.Item1;
            TLChannelFull fullInfo = channelInfo.Item2;
            if (fullInfo?.LinkedChatId == null)
            {
                throw new NoLinkedChatException();
            }
            else
            {
                await ReplyInDiscussion(channel, message);
            }
        }

        private async Task ReplyInDiscussion(TLChannel channel, string message)
        {
            TLMessage lastMessage = await GetLastMessage(channel);

            DiscussionInfo discussionInfo = await GetDiscussionInfo(channel, lastMessage);
            if (discussionInfo == null)
                throw new NoDiscussionsException();

            TLChannel targetChannel = new TLChannel()
            {
                Id = discussionInfo.Id,
                AccessHash = discussionInfo.AccessHash
            };
            await JoinChannel(targetChannel);

            await ReplyTo(targetChannel, message, discussionInfo.PostId);
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
            TLDiscussionMessage discussionMessageResponse =
                await SendRequestAsync<TLDiscussionMessage>(discussionMessageRequest);
            TLMessage discussionMessage = discussionMessageResponse.Messages.Where(x => x is TLMessage)
                .Cast<TLMessage>().FirstOrDefault();
            if (discussionMessage == null)
            {
                return null;
            }
            
            int discussionChatId = ((TLPeerChannel) discussionMessage.PeerId).ChannelId;
            
            TLChannel discussionsChannel =
                discussionMessageResponse.Chats
                    .Where(x => x is TLChannel possibleChannel && possibleChannel.Id == discussionChatId)
                    .Cast<TLChannel>()
                    .FirstOrDefault();
            if (discussionsChannel == null)
                return null;
            
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
            TLRequestResolveUsername requestResolveUsername = new TLRequestResolveUsername()
            {
                Username = username
            };

            TLResolvedPeer peer = await SendRequestAsync<TLResolvedPeer>(requestResolveUsername);

            TLChannel channel = (TLChannel) peer.Chats.First();
            return channel;
        }
        
        public async Task<Tuple<TLChannel, TLChannelFull>> GetFullChannel(string username)
        {
            if (!_authorized)
                throw new UnauthorizedAccessException("Not authorized");

            TLChannel channel = await GetChannel(username);

            TLRequestGetFullChannel requestGetFullChannel = new TLRequestGetFullChannel()
            {
                Channel = new TLInputChannel()
                {
                    ChannelId = channel.Id,
                    AccessHash = channel.AccessHash ?? 0
                }
            };

            TeleSharp.TL.Messages.TLChatFull chatFull = await SendRequestAsync<TeleSharp.TL.Messages.TLChatFull>(requestGetFullChannel);
            return new Tuple<TLChannel, TLChannelFull>(channel, chatFull.FullChat as TLChannelFull);
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
        
        public async Task<T> SendRequestAsync<T>(TLMethod methodToExecute, CancellationToken token = default(CancellationToken))
        {
            await Task.Delay(RandomHelper.GetRandomDelay(_delay.Item1, _delay.Item2));
            return await _client.SendRequestAsync<T>(methodToExecute, token);
        }
    }

    class DiscussionInfo
    {
        public int Id { get; set; }
        public long? AccessHash { get; set; }
        public int PostId { get; set; }
    }

    [Serializable]
    public class NoLinkedChatException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public NoLinkedChatException()
        {
        }

        public NoLinkedChatException(string message) : base(message)
        {
        }

        public NoLinkedChatException(string message, Exception inner) : base(message, inner)
        {
        }

        protected NoLinkedChatException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
    
    [Serializable]
    public class NoDiscussionsException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public NoDiscussionsException()
        {
        }

        public NoDiscussionsException(string message) : base(message)
        {
        }

        public NoDiscussionsException(string message, Exception inner) : base(message, inner)
        {
        }

        protected NoDiscussionsException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
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
