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

using Cortex.Server.Game.Achievements;

namespace Cortex.Server.Game.Rooms.Furnitures.Logics {
    class GameRoomFurnitureBanzaiTile : IGameRoomFurnitureLogic {
        public GameRoomFurniture Furniture { get; set; }

        public int Team = 0;
        public int Step = 0;

        public bool Locked = true;

        public void OnUserEnter(GameRoomUser user) {
            if(Locked)
                return;
                
            int team = GameRoomFurnitureBanzai.GetUserTeam(user);

            if(team == 0)
                return;
                
            if(Step == 2)
                return;

            if(Team != team) {
                Team = team;

                Step = 0;
            }
            else if(Step < 2) {
                Step++;

                if(Step == 2) {
                    Locked = true;

                    GameAchievementManager.AddScore(user.User,  GameAchievements.BattleBallTilesLocked, 1);

                    int unlocked = 0;

                    foreach(GameRoomFurniture furniture in Furniture.Room.Furnitures.Where(x => x.Logic is GameRoomFurnitureBanzaiTile)) {
                        GameRoomFurnitureBanzaiTile tile = furniture.Logic as GameRoomFurnitureBanzaiTile;

                        if(!tile.Locked)
                            unlocked++;
                    }

                    if(unlocked == 0) {
                        foreach(GameRoomFurniture furniture in Furniture.Room.Furnitures)
                            furniture.Logic.OnGameStop();

                        GameRoomFurnitureBanzai.OnGameStop(Furniture);
                    }
                }
            }

            Furniture.Animation = (team * 3) + Step;
            Furniture.Room.Actions.AddEntity(Furniture.Id, new GameRoomFurnitureAnimation(Furniture, Furniture.Animation));

            foreach(GameRoomFurniture furniture in Furniture.Room.Furnitures.Where(x => x.Logic is GameRoomFurnitureBanzaiScore))
                (furniture.Logic as GameRoomFurnitureBanzaiScore).UpdateScore();
        }

        public void OnGameStart() {
            Locked = false;

            Team = 0;
            Step = 1;

            if(Furniture.Animation != 1) {
                Furniture.Animation = 1;
                Furniture.Room.Actions.AddEntity(Furniture.Id, new GameRoomFurnitureAnimation(Furniture, Furniture.Animation));
            }
        }

        public void OnGameStop() {
            Locked = true;
        }
    }
}
