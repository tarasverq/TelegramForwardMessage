﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TeleSharp.TL;
using TeleSharp.TL.Account;
using TeleSharp.TL.Auth;
using TeleSharp.TL.Contacts;
using TeleSharp.TL.Help;
using TeleSharp.TL.Messages;
using TeleSharp.TL.Upload;
using TLSharp.Core.Auth;
using TLSharp.Core.Exceptions;
using TLSharp.Core.MTProto.Crypto;
using TLSharp.Core.Network;
using TLSharp.Core.Network.Exceptions;
using TLSharp.Core.Utils;
using TLAuthorization = TeleSharp.TL.Auth.TLAuthorization;

namespace TLSharp.Core
{
    public class TelegramClient : IDisposable
    {
        private MtProtoSender sender;
        private TcpTransport transport;
        private string apiHash = String.Empty;
        private int apiId = 0;
        private Session session;
        private List<TLDcOption> dcOptions;
        private TcpClientConnectionHandler handler;
        private DataCenterIPVersion dcIpVersion;

        public Session Session
        {
            get { return session; }
        }

        /// <summary>
        /// Creates a new TelegramClient
        /// </summary>
        /// <param name="apiId">The API ID provided by Telegram. Get one at https://my.telegram.org </param>
        /// <param name="apiHash">The API Hash provided by Telegram. Get one at https://my.telegram.org </param>
        /// <param name="store">An ISessionStore object that will handle the session</param>
        /// <param name="sessionUserId">The name of the session that tracks login info about this TelegramClient connection</param>
        /// <param name="handler">A delegate to invoke when a connection is needed and that will return a TcpClient that will be used to connect</param>
        /// <param name="dcIpVersion">Indicates the preferred IpAddress version to use to connect to a Telegram server</param>
        public TelegramClient(int apiId, string apiHash,
            ISessionStore store = null, string sessionUserId = "session", TcpClientConnectionHandler handler = null,
            DataCenterIPVersion dcIpVersion = DataCenterIPVersion.Default)
        {
            if (apiId == default(int))
                throw new MissingApiConfigurationException("API_ID");
            if (string.IsNullOrEmpty(apiHash))
                throw new MissingApiConfigurationException("API_HASH");

            if (store == null)
                store = new FileSessionStore();

            this.apiHash = apiHash;
            this.apiId = apiId;
            this.handler = handler;
            this.dcIpVersion = dcIpVersion;

            session = Session.TryLoadOrCreateNew(store, sessionUserId);
            transport = new TcpTransport (session.DataCenter.Address, session.DataCenter.Port, this.handler);
        }

        public async Task ConnectAsync(bool reconnect = false, CancellationToken token = default(CancellationToken))
        {
            token.ThrowIfCancellationRequested();

            if (session.AuthKey == null || reconnect)
            {
                var result = await Authenticator.DoAuthentication(transport, token).ConfigureAwait(false);
                session.AuthKey = result.AuthKey;
                session.TimeOffset = result.TimeOffset;
            }

            sender = new MtProtoSender(transport, session);

            //set-up layer
            var config = new TLRequestGetConfig();
            var request = new TLRequestInitConnection()
            {
                ApiId = apiId,
                AppVersion = "0.7.0",
                DeviceModel = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36",
                LangCode = "en-US",
                Query = config,
                SystemVersion = "Win32",
                LangPack = "",
                SystemLangCode = "en-US",
                
            };
            var invokewithLayer = new TLRequestInvokeWithLayer() { Layer = 121, Query = request };
            await sender.Send(invokewithLayer, token).ConfigureAwait(false);
            await sender.Receive(invokewithLayer, token).ConfigureAwait(false);

            dcOptions = ((TLConfig)invokewithLayer.Response).DcOptions.ToList();
        }

        private async Task ReconnectToDcAsync(int dcId, CancellationToken token = default(CancellationToken))
        {
            token.ThrowIfCancellationRequested();

            if (dcOptions == null || !dcOptions.Any())
                throw new InvalidOperationException($"Can't reconnect. Establish initial connection first.");

            TLExportedAuthorization exported = null;
            if (session.TLUser != null)
            {
                TLRequestExportAuthorization exportAuthorization = new TLRequestExportAuthorization() { DcId = dcId };
                exported = await SendRequestAsync<TLExportedAuthorization>(exportAuthorization, token).ConfigureAwait(false);
            }

            IEnumerable<TLDcOption> dcs;
            if (dcIpVersion == DataCenterIPVersion.OnlyIPv6)
                dcs = dcOptions.Where(d => d.Id == dcId && d.Ipv6); // selects only ipv6 addresses 	
            else if (dcIpVersion == DataCenterIPVersion.OnlyIPv4)
                dcs = dcOptions.Where(d => d.Id == dcId && !d.Ipv6); // selects only ipv4 addresses
            else
                dcs = dcOptions.Where(d => d.Id == dcId); // any

            dcs = dcs.Where(d => !d.MediaOnly);

            TLDcOption dc;
            if (dcIpVersion != DataCenterIPVersion.Default)
            {
                if (!dcs.Any())
                    throw new Exception($"Telegram server didn't provide us with any IPAddress that matches your preferences. If you chose OnlyIPvX, try switch to PreferIPvX instead.");
                dcs = dcs.OrderBy(d => d.Ipv6);
                dc = dcIpVersion == DataCenterIPVersion.PreferIPv4 ? dcs.First() : dcs.Last(); // ipv4 addresses are at the beginning of the list because it was ordered
            }
            else
                dc = dcs.First();
            
            var dataCenter = new DataCenter (dcId, dc.IpAddress, dc.Port);

            transport = new TcpTransport(dc.IpAddress, dc.Port, handler);
            session.DataCenter = dataCenter;

            await ConnectAsync(true, token).ConfigureAwait(false);

            if (session.TLUser != null)
            {
                TLRequestImportAuthorization importAuthorization = new TLRequestImportAuthorization() { Id = exported.Id, Bytes = exported.Bytes };
                var imported = await SendRequestAsync<TLAuthorization>(importAuthorization, token).ConfigureAwait(false);
                OnUserAuthenticated((TLUser)imported.User);
            }
        }

        private async Task RequestWithDcMigration(TLMethod request, CancellationToken token = default(CancellationToken))
        {
            if (sender == null)
                throw new InvalidOperationException("Not connected!");

            var completed = false;
            while(!completed)
            {
                try
                {
                    await sender.Send(request, token).ConfigureAwait(false);
                    await sender.Receive(request, token).ConfigureAwait(false);
                    completed = true;
                }
                catch(DataCenterMigrationException e)
                {
                    if (session.DataCenter.DataCenterId.HasValue &&
                        session.DataCenter.DataCenterId.Value == e.DC)
                    {
                        throw new Exception($"Telegram server replied requesting a migration to DataCenter {e.DC} when this connection was already using this DataCenter", e);
                    }

                    await ReconnectToDcAsync(e.DC, token).ConfigureAwait(false);
                    // prepare the request for another try
                    request.ConfirmReceived = false;
                }
            }
        }

        public bool IsUserAuthorized()
        {
            return session.TLUser != null;
        }

      

        public async Task<string> SendCodeRequestAsync(string phoneNumber, CancellationToken token = default(CancellationToken))
        {
            if (String.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentNullException(nameof(phoneNumber));

            var request = new TLRequestSendCode() { PhoneNumber = phoneNumber, ApiId = apiId, ApiHash = apiHash, Settings = new TLCodeSettings()};

            await RequestWithDcMigration(request, token).ConfigureAwait(false);

            return request.Response.PhoneCodeHash;
        }

        public async Task<TLUser> MakeAuthAsync(string phoneNumber, string phoneCodeHash, string code, CancellationToken token = default(CancellationToken))
        {
            if (String.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentNullException(nameof(phoneNumber));

            if (String.IsNullOrWhiteSpace(phoneCodeHash))
                throw new ArgumentNullException(nameof(phoneCodeHash));

            if (String.IsNullOrWhiteSpace(code))
                throw new ArgumentNullException(nameof(code));
            
            var request = new TLRequestSignIn() { PhoneNumber = phoneNumber, PhoneCodeHash = phoneCodeHash, PhoneCode = code };

            await RequestWithDcMigration(request, token).ConfigureAwait(false);

            OnUserAuthenticated(((TLUser)request.Response.User));

            return ((TLUser)request.Response.User);
        }
        
        public async Task<TLPassword> GetPasswordSetting(CancellationToken token = default(CancellationToken))
        {
            var request = new TLRequestGetPassword();

            await RequestWithDcMigration(request, token).ConfigureAwait(false);

            return (TLPassword)request.Response;
        }

        public async Task<T> SendRequestAsync<T>(TLMethod methodToExecute, CancellationToken token = default(CancellationToken))
        {
            await RequestWithDcMigration(methodToExecute, token).ConfigureAwait(false);

            var result = methodToExecute.GetType().GetProperty("Response").GetValue(methodToExecute);

            return (T)result;
        }

        internal async Task<T> SendAuthenticatedRequestAsync<T> (TLMethod methodToExecute, CancellationToken token = default(CancellationToken))
        {
            if (!IsUserAuthorized())
                throw new InvalidOperationException("Authorize user first!");

            return await SendRequestAsync<T>(methodToExecute, token)
                .ConfigureAwait(false);
        }

        public async Task<TLUser> UpdateUsernameAsync(string username, CancellationToken token = default(CancellationToken))
        {
            var req = new TLRequestUpdateUsername { Username = username };

            return await SendAuthenticatedRequestAsync<TLUser>(req, token)
                .ConfigureAwait(false);
        }

        public async Task<bool> CheckUsernameAsync(string username, CancellationToken token = default(CancellationToken))
        {
            var req = new TLRequestCheckUsername { Username = username };

            return await SendAuthenticatedRequestAsync<bool>(req, token)
                .ConfigureAwait(false);
        }

        public async Task<TLImportedContacts> ImportContactsAsync(IReadOnlyList<TLInputPhoneContact> contacts, CancellationToken token = default(CancellationToken))
        {
            var req = new TLRequestImportContacts { Contacts = new TLVector<TLInputPhoneContact>(contacts)};

            return await SendAuthenticatedRequestAsync<TLImportedContacts>(req, token)
                .ConfigureAwait(false);
        }

        public async Task<bool> DeleteContactsAsync(IReadOnlyList<TLAbsInputUser> users, CancellationToken token = default(CancellationToken))
        {
            var req = new TLRequestDeleteContacts {Id = new TLVector<TLAbsInputUser>(users)};

            return await SendAuthenticatedRequestAsync<bool>(req, token)
                .ConfigureAwait(false);
        }

        public async Task<TLContacts> GetContactsAsync(CancellationToken token = default(CancellationToken))
        {
            var req = new TLRequestGetContacts() { Hash = 0 };

            return await SendAuthenticatedRequestAsync<TLContacts>(req, token)
                .ConfigureAwait(false);
        }

        public async Task<TLAbsUpdates> SendMessageAsync(TLAbsInputPeer peer, string message, CancellationToken token = default(CancellationToken))
        {
            return await SendAuthenticatedRequestAsync<TLAbsUpdates>(
                    new TLRequestSendMessage()
                    {
                        Peer = peer,
                        Message = message,
                        RandomId = Helpers.GenerateRandomLong()
                    }, token)
                .ConfigureAwait(false);
        }

        public async Task<Boolean> SendTypingAsync(TLAbsInputPeer peer, CancellationToken token = default(CancellationToken))
        {
            var req = new TLRequestSetTyping()
            {
                Action = new TLSendMessageTypingAction(),
                Peer = peer
            };
            return await SendAuthenticatedRequestAsync<Boolean>(req, token)
                .ConfigureAwait(false);
        }

        public async Task<TLAbsDialogs> GetUserDialogsAsync(int offsetDate = 0, int offsetId = 0, TLAbsInputPeer offsetPeer = null, int limit = 100, CancellationToken token = default(CancellationToken))
        {
            if (offsetPeer == null)
                offsetPeer = new TLInputPeerSelf();

            var req = new TLRequestGetDialogs()
            { 
                OffsetDate = offsetDate, 
                OffsetId = offsetId, 
                OffsetPeer = offsetPeer, 
                Limit = limit
            };
            return await SendAuthenticatedRequestAsync<TLAbsDialogs>(req, token)
                .ConfigureAwait(false);
        }

        public async Task<TLAbsUpdates> SendUploadedPhoto(TLAbsInputPeer peer, TLAbsInputFile file, string caption, CancellationToken token = default(CancellationToken))
        {
            return await SendAuthenticatedRequestAsync<TLAbsUpdates>(new TLRequestSendMedia()
                {
                    RandomId = Helpers.GenerateRandomLong(),
                    Background = false,
                    ClearDraft = false,
                    Media = new TLInputMediaUploadedPhoto() { File = file },
                    Peer = peer
                }, token)
                .ConfigureAwait(false);
        }

        public async Task<TLAbsUpdates> SendUploadedDocument(
            TLAbsInputPeer peer, TLAbsInputFile file, string caption, string mimeType, TLVector<TLAbsDocumentAttribute> attributes, CancellationToken token = default(CancellationToken))
        {
            return await SendAuthenticatedRequestAsync<TLAbsUpdates>(new TLRequestSendMedia()
                {
                    RandomId = Helpers.GenerateRandomLong(),
                    Background = false,
                    ClearDraft = false,
                    Media = new TLInputMediaUploadedDocument()
                    {
                        File = file,
                        MimeType = mimeType,
                        Attributes = attributes
                    },
                    Peer = peer
                }, token)
                .ConfigureAwait(false);
        }

        public async Task<TLFile> GetFile(TLAbsInputFileLocation location, int filePartSize, int offset = 0, CancellationToken token = default(CancellationToken))
        {
            TLFile result = await SendAuthenticatedRequestAsync<TLFile>(new TLRequestGetFile
                {
                    Location = location,
                    Limit = filePartSize,
                    Offset = offset
                }, token)
                .ConfigureAwait(false);
            return result;
        }

        public async Task SendPingAsync(CancellationToken token = default(CancellationToken))
        {
            await sender.SendPingAsync(token)
                .ConfigureAwait(false);
        }

        public async Task<TLAbsMessages> GetHistoryAsync(TLAbsInputPeer peer, int offsetId = 0, int offsetDate = 0, int addOffset = 0, int limit = 100, int maxId = 0, int minId = 0, CancellationToken token = default(CancellationToken))
        {
            var req = new TLRequestGetHistory()
            {
                Peer = peer,
                OffsetId = offsetId,
                OffsetDate = offsetDate,
                AddOffset = addOffset,
                Limit = limit,
                MaxId = maxId,
                MinId = minId
            };
            return await SendAuthenticatedRequestAsync<TLAbsMessages>(req, token)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Serch user or chat. API: contacts.search#11f812d8 q:string limit:int = contacts.Found;
        /// </summary>
        /// <param name="q">User or chat name</param>
        /// <param name="limit">Max result count</param>
        /// <returns></returns>
        public async Task<TLFound> SearchUserAsync(string q, int limit = 10, CancellationToken token = default(CancellationToken))
        {
            var r = new TeleSharp.TL.Contacts.TLRequestSearch
            {
                Q = q,
                Limit = limit
            };

            return await SendAuthenticatedRequestAsync<TLFound>(r, token)
                .ConfigureAwait(false);
        }

        private void OnUserAuthenticated(TLUser TLUser)
        {
            session.TLUser = TLUser;
            session.SessionExpires = int.MaxValue;

            session.Save();
        }

        public bool IsConnected
        {
            get
            {
                if (transport == null)
                    return false;
                return transport.IsConnected;
            }
        }

        public void Dispose()
        {
            if (transport != null)
            {
                transport.Dispose();
                transport = null;
            }
        }
    }
}
