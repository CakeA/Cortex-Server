using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using MySql.Data.MySqlClient;

using Cortex.Server.Game;
using Cortex.Server.Game.Users;
using Cortex.Server.Game.Users.Furnitures;

using Cortex.Server.Game.Rooms;
using Cortex.Server.Game.Rooms.Users;
using Cortex.Server.Game.Rooms.Actions;

using Cortex.Server.Game.Furnitures;

using Cortex.Server.Socket.Clients;
using Cortex.Server.Socket.Events;
using Cortex.Server.Socket.Messages;

namespace Cortex.Server.Game.Rooms.Furnitures.Events {
    class OnRoomFurniturePickup : ISocketEvent {
        public string Event => "OnRoomFurniturePickup";

        public int Execute(SocketClient client, JToken data) {
            if(client.User.Room == null)
                return 0;

            GameRoomUser roomUser = client.User.Room.GetUser(client.User.Id);

            if(!roomUser.HasRights())
                return 0;
            
            int id = data.ToObject<int>();

            GameRoomFurniture roomFurniture = client.User.Room.Furnitures.Find(x => x.Id == id);

            if(roomFurniture == null)
                return 0;

            client.User.Room.Furnitures.Remove(roomFurniture);

            roomFurniture.UserFurniture.Room = 0;

            client.User.Room.Send(new SocketMessage("OnRoomEntityRemove", new { furnitures = roomFurniture.Id }).Compose());

            client.Send(new SocketMessage("OnRoomFurniturePickup", roomFurniture.Id).Compose());

            using(MySqlConnection connection = new MySqlConnection(Program.Connection)) {
                connection.Open();

                using(MySqlCommand command = new MySqlCommand("DELETE FROM room_furnitures WHERE id = @id", connection)) {
                    command.Parameters.AddWithValue("@id", roomFurniture.Id);

                    command.ExecuteNonQuery();
                }

                using(MySqlCommand command = new MySqlCommand("UPDATE user_furnitures SET room = 0 WHERE id = @id", connection)) {
                    command.Parameters.AddWithValue("@id", roomFurniture.UserFurniture.Id);

                    command.ExecuteNonQuery();
                }
            }

            GameUser owner = Game.GetUser(roomFurniture.UserFurniture.User);

            if(owner != null)
                owner.Client.Send(new SocketMessage("OnUserFurnitureUpdate", owner.GetFurnitureMessages(roomFurniture.UserFurniture.Furniture.Id)).Compose());

            foreach(GameRoomFurniture stacked in roomFurniture.Room.Furnitures.FindAll(x => x.Position.Row == roomFurniture.Position.Row && x.Position.Column == roomFurniture.Position.Column)) {
                if(stacked.Id == roomFurniture.Id)
                    continue;

                if(stacked.Logic == null)
                    continue;

                stacked.Logic.OnFurnitureLeave(roomFurniture);
            }

            return 1;
        }
    }
}
