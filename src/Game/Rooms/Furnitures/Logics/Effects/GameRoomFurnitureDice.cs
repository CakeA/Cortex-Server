using System;
using System.Linq;
using System.Timers;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using MySql.Data.MySqlClient;

using Cortex.Server.Game.Furnitures;

using Cortex.Server.Game.Users;
using Cortex.Server.Game.Users.Furnitures;

using Cortex.Server.Game.Rooms.Actions;
using Cortex.Server.Game.Rooms.Users;

using Cortex.Server.Game.Rooms.Furnitures.Actions;

using Cortex.Server.Events;
using Cortex.Server.Socket.Messages;

namespace Cortex.Server.Game.Rooms.Furnitures.Logics {
    class GameRoomFurnitureDice : IGameRoomFurnitureLogic {
        public GameRoomFurniture Furniture { get; set; }

        public void OnUserUse(GameRoomUser user, JToken data) {
            if(Furniture.Position.GetDistance(user.Position) > 1)
                return;

            string tag = data["tag"].ToString();

            if(tag == "activate") {
                if(Furniture.Animation == -1)
                    return;

                Furniture.SetAnimation(-1, false);

                Timer timer = new Timer(1000);

                timer.Elapsed += (a, b) => {
                    int dice = new Random().Next(1, 7);

                    Furniture.SetAnimation(dice, true);

                    timer.Stop();
                };

                timer.Start();
            }
            else if(tag == "deactivate") {
                if(Furniture.Animation == 0)
                    return;

                Furniture.SetAnimation(0, true);
            }
        }
    }
}
