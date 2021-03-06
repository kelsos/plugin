﻿using System;
using MusicBeeRemote.Core.ApiAdapters;
using MusicBeeRemote.Core.Events;
using MusicBeeRemote.Core.Events.Status.Internal;
using MusicBeeRemote.Core.Model.Entities;
using MusicBeeRemote.Core.Network;
using Newtonsoft.Json.Linq;
using TinyMessenger;

namespace MusicBeeRemote.Core.Commands.Requests.PlayerStateCommands
{
    public class RequestScrobble : ICommand
    {
        private readonly ITinyMessengerHub _hub;
        private readonly IPlayerApiAdapter _apiAdapter;

        public RequestScrobble(ITinyMessengerHub hub, IPlayerApiAdapter apiAdapter)
        {
            _hub = hub;
            _apiAdapter = apiAdapter;
        }

        public void Execute(IEvent receivedEvent)
        {
            if (receivedEvent == null)
            {
                throw new ArgumentNullException(nameof(receivedEvent));
            }

            if (receivedEvent.Data is JToken token &&
                ((string)token).Equals("toggle", StringComparison.InvariantCultureIgnoreCase))
            {
                _apiAdapter.ToggleScrobbling();
            }

            var message = new SocketMessage(Constants.PlayerScrobble, _apiAdapter.ScrobblingEnabled());
            _hub.Publish(new PluginResponseAvailableEvent(message));
        }
    }
}
