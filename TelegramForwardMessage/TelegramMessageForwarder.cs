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
using TLSharp.Core.Network;
using TLSharp.Core.Network.Exceptions;
using TLSharp.Core.Utils;

namespace TelegramForwardMessage
{
    public interface ITelegramMessageForwarder : IDisposable
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

        public TelegramMessageForwarder(int clientId, string clientHash, string phone, Tuple<int, int> delay,
            TcpClientConnectionHandler handler = null)
        {
            _phone = phone;
            _delay = delay;

            _client = new TelegramClient(clientId, clientHash, null, phone, handler);
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
            try
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
            catch (InvalidOperationException invalidOperationException)
            {
                switch (invalidOperationException.Message.Trim())
                {
                    case "USER_BANNED_IN_CHANNEL":
                    case "CHAT_WRITE_FORBIDDEN":
                        throw new UserBanedInChannelException($"User {_phone} banned in channel {channelUsername}");
                    default:
                        throw;
                }
            }
        }

        private async Task ReplyInDiscussion(TLChannel channel, string message, TLChannel chat)
        {
            TLMessage lastMessage = await GetLastMessage(channel);

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
                Tuple<TLChannel, TLChannelFull, TLChannel> chatInfo = await GetFullChannel(new TLInputChannel()
                    {ChannelId = chat.Id, AccessHash = chat.AccessHash ?? 0});
                targetChannel = new TLChannel()
                {
                    Id = chatInfo.Item1.Id,
                    AccessHash = chatInfo.Item1.AccessHash
                };
                targetPostId = chatInfo.Item2.PinnedMsgId ?? 0;
            }

            await JoinChannel(channel);
            await Delay();
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
                when (invalidOperationException.Message == "USERNAME_NOT_OCCUPIED"
                      || invalidOperationException.Message == "USERNAME_INVALID")
            {
                throw new ChannelNotFoundException(invalidOperationException.Message, invalidOperationException)
                    {ChannelName = username};
            }


            TLChannel channel =
                peer.Chats.FirstOrDefault(x => x is TLChannel channelFilter && channelFilter.Username == username) as
                    TLChannel;
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
                    resp.Chats.First(x => x is TLChannel channelFilter /* && channelFilter.Title == invite.Title*/) as
                        TLChannel;
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

            TeleSharp.TL.Messages.TLChatFull chatFull =
                await SendRequestAsync<TeleSharp.TL.Messages.TLChatFull>(requestGetFullChannel);


            TLChannelFull channelFull = chatFull.FullChat as TLChannelFull;
            TLChannel channel = chatFull.Chats.First(x =>
                x is TLChannel channelFilter && channelFilter.Id == channelFull?.Id) as TLChannel;
            TLChannel chat = chatFull.Chats.FirstOrDefault(x =>
                x is TLChannel channelFilter && channelFilter.Id == channelFull?.LinkedChatId) as TLChannel;

            return new Tuple<TLChannel, TLChannelFull, TLChannel>(channel, channelFull, chat);
        }


        public CancellationTokenSource TokenSource = new CancellationTokenSource();

        public void RunOnlineSending()
        {
            CancellationToken ct = TokenSource.Token;

            Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        if (ct.IsCancellationRequested)
                            return;

                        try
                        {
                            await Task.Delay(60000, ct);
                            await SetOnline(false);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            return;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return;
                }
            }, ct);
        }

        public async Task SetOnline(bool offline = false)
        {
            TLRequestUpdateStatus requestUpdateStatus = new TLRequestUpdateStatus()
            {
                Offline = offline,
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
                await Delay();
                return await _client.SendRequestAsync<T>(methodToExecute, token);
            }
            catch (FloodException e)
            {
                throw new FloodException(e.TimeToWait,
                    System.Text.Json.JsonSerializer.Serialize(_methodCounter, options:
                        new System.Text.Json.JsonSerializerOptions() {WriteIndented = true}), e);
            }
        }

        private async Task Delay()
        {
            await Task.Delay(RandomHelper.GetRandomDelay(_delay.Item1, _delay.Item2));
        }

        private Dictionary<string, int> _methodCounter = new Dictionary<string, int>();
        public IReadOnlyDictionary<string, int> MethodCounter => _methodCounter;

        public void ClearMethodCounter()
        {
            _methodCounter = new();
        }

        public void Dispose()
        {
            _client?.Dispose();
            TokenSource?.Cancel();
            TokenSource?.Dispose();
        }
    }

    class DiscussionInfo
    {
        public int Id { get; set; }
        public long? AccessHash { get; set; }
        public int PostId { get; set; }
    }
}