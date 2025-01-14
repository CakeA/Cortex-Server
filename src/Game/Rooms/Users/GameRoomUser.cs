using System;
using System.Collections.Generic;

using Newtonsoft.Json;

using Cortex.Server.Game.Users;
using Cortex.Server.Game.Rooms.Users.Actions;
using Cortex.Server.Game.Rooms.Actions;

using Cortex.Server.Socket.Messages;

namespace Cortex.Server.Game.Rooms.Users {
    class GameRoomUser {
        [JsonProperty("id")]
        public int Id;

        [JsonIgnore]
        public GameUser User;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("effect")]
        public int Effect;

        [JsonProperty("position")]
        public GameRoomPoint Position;

        [JsonProperty("figure")]
        public string Figure;

        [JsonProperty("actions")]
        public List<string> Actions = new List<string>();

        public GameRoomUser(GameUser user) {
            Id = user.Id;
            
            User = user;

            Name = user.Name;

            Position = new GameRoomPoint();

            Figure = user.Figure;

            //User.Room.Events.AddUser(this, new GameRoomUserAction(this, "GestureAngry", GameRoomUserActionType.Add));
        }

        public bool HasRights() {
            if(Id == User.Room.User)
                return true;

            if(User.Room.Rights.Contains(Id))
                return true;

            return false;
        }

        public void SetEffect(int effect) {
            Effect = effect;

            User.Room.Send(new SocketMessage("OnRoomUserEffect", new {
                id = Id,

                effect = effect
            }).Compose());
        }
    }
}
