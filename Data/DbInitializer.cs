﻿using Assetify.Models;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Assetify.Data
{
    public class DbInitializer
    {
        public static void Initialize(AssetifyContext context)
        {
            context.Database.EnsureCreated();
            if(context.Assets.Any()) { return; }

            var address = new Address[]
            {
                new Address{City="Holon",Street="Emek Dotan", Building="3", Full="Emek Dotan 3, Holon", IsPublic=false, Neighborhood="Kiryat sharet", Latitude=222, Longitude=222 },
                new Address{City="Maor",Street="Azait", Building="101", Full="Azait 101, Moshav Maor", IsPublic=true, Neighborhood="", Latitude=111, Longitude=111 },
                new Address{City="Tel Aviv",Street="Begin Road", Building="150", Full="", IsPublic=false, Neighborhood="", Latitude=444, Longitude=444 },
                new Address{City="Bat Yam",Street="Hahazmaot", Building="150", Full="", IsPublic=false, Neighborhood="", Latitude=777, Longitude=777 },
                new Address{City="Harish",Street="Turkiz", Building="9", Full="", IsPublic=true, Neighborhood="Avnei Hen", Latitude=999, Longitude=999 },
                new Address{City="Holon",Street="Harokmim", Building="26", Full="", IsPublic=true, Neighborhood="", Latitude=555, Longitude=555 }//,
              //  new Address{City="Tibiria",Street="Oranim", Building="1", Full="Oranim 1, Tibiria", IsPublic=false, Neighborhood="Ramot Tibiria", Latitude=333, Longitude=333 }
            };

            foreach (Address a in address)
            {
                context.Addresses.Add(a);
            }

            context.SaveChanges();

            DateTime now = DateTime.Now;
            var users = new User[]
            {
             new User{Email = "test@c.com", Password = "123", FirstName = "test", LastName = "Stam", Phone = "052-2222222", IsVerified = true, ProfileImgPath = "data:image/png;base64,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", LastSeenFavorite = now, LastSeenMessages = now },
             new User{Email = "test2@c.com", Password = "1234", FirstName = "exam", LastName = "Publisher", Phone = "052-33333333", IsVerified = false, ProfileImgPath = "data:image/png;base64,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", LastSeenFavorite = now, LastSeenMessages = now },
             new User{Email = "galhrrs@gmail.com", Password = "polpol", FirstName = "Gal", LastName = "Harris", Phone = "055-6633084", IsVerified = false, ProfileImgPath = "no image", LastSeenFavorite = now, LastSeenMessages = now},
             new User{Email = "galhrrsa@gmail.com", Password = "polpola", FirstName = "Gala", LastName = "Harrisa", Phone = "055-6633085", IsVerified = false, ProfileImgPath = "no image", LastSeenFavorite = now, LastSeenMessages = now, IsAdmin = true}
            };
            foreach (User u in users)
            {
                context.Users.Add(u);
            }
            context.SaveChanges();

            DateTime twoDaysago = now.AddDays(-2);
            DateTime fourDaysago = now.AddDays(-4);
            var assets = new Asset[]
            {
                new Asset{AddressID=4, CreatedAt=twoDaysago, Price=1350000, BalconySize=17, Condition=AssetCondition.Renovated, Description="Beach View", EntryDate=now, Floor=4, IsAircondition=true, IsActive=true, IsBalcony=true, IsCommercial=false, TypeId=AssetType.Apartment, Furnished=FurnishedType.Partial, IsElevator=true, IsBars=false, IsImmediate=true, IsMamad=true, IsNearBeach=true, TotalFloor=6, IsAccessible=true, IsForSell=true, IsKosherBoiler=false, IsOnPillars=false, IsKosherKitchen=false, IsNearLightTrainStation=true, IsNearTrainStation=true, IsPandorDoors=false, IsRealtyCommission=false, IsRenovated=true, IsRoomates=false, IsStorage=false, Rooms=3, Size=70, IsTerrace=false},
                new Asset{AddressID=4, CreatedAt=fourDaysago,Price=1400000, BalconySize=17, Condition=AssetCondition.New, Description="Big Garden", EntryDate=now, Floor=0, IsAircondition=true, IsActive=true, IsBalcony=true, IsCommercial=false, TypeId=AssetType.GardenApartment, Furnished=FurnishedType.Full, IsElevator=true, IsBars=false, IsImmediate=true, IsMamad=true, IsNearBeach=false, TotalFloor=6, IsAccessible=true, IsForSell=true, IsKosherBoiler=false, IsOnPillars=false, IsKosherKitchen=false, IsNearLightTrainStation=true, IsNearTrainStation=true, IsPandorDoors=true, IsRealtyCommission=true, IsRenovated=false, IsRoomates=false, IsStorage=true, Rooms=4, Size=110, IsTerrace=false, GardenSize=100}
            };

            foreach (Asset a in assets)
            {
                context.Assets.Add(a);
            }

            context.SaveChanges();

            var userAssets = new UserAsset[]
            {
                new UserAsset{ UserID=2, AssetID=1, ActionTime=twoDaysago, ActionID = (int)ActionType.PUBLISH, IsSeen=true},
                new UserAsset{ UserID=2, AssetID=2, ActionTime=fourDaysago, ActionID = (int)ActionType.PUBLISH, IsSeen=true},
                new UserAsset{ UserID=1, AssetID=2, ActionTime=now, ActionID = (int)ActionType.LIKE, IsSeen=true},
            };

            foreach (UserAsset ua in userAssets)
            {
                context.UserAsset.Add(ua);
            }

            context.SaveChanges();
            var assetsOrientations = new AssetOrientation[]
            {
                new AssetOrientation{AssetID=1, Orientation=OrientationType.West},
                new AssetOrientation{AssetID=1, Orientation=OrientationType.North},
                new AssetOrientation{AssetID=2, Orientation=OrientationType.North},
                new AssetOrientation{AssetID=2, Orientation=OrientationType.West},
                new AssetOrientation{AssetID=2, Orientation=OrientationType.East},
            };

            foreach (AssetOrientation o in assetsOrientations)
            {
                context.Orientations.Add(o);
            }

            var assetsImages = new AssetImage[]
            {
                new AssetImage{AssetID=2, Path="https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcQv27RpcOyfXCXWNQAIP4ZCE1wog76uF57dbQ&usqp=CAU", Type=""},
                new AssetImage{AssetID=2, Path="https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcRVKDRaQ_xwdWIn8fZ8i6igk4f1dE_fPjLgZw&usqp=CAU", Type=""},
                new AssetImage{AssetID=2, Path="https://q-xx.bstatic.com/xdata/images/hotel/840x460/134503030.jpg?k=84fc1387bcaaf7bed45609874b06ecacc2ca723de4046de353b2dca04ce937ca&o=", Type=""},
                new AssetImage{AssetID=2, Path="https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcT4zjHfKdPkpEAwU7K5wzQSGCvjkIqGAmNh0A&usqp=CAU", Type=""},
                new AssetImage{AssetID=1, Path="data:image/jpeg;base64,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", Type=""},
                new AssetImage{AssetID=1, Path="https://imgcy.trivago.com/c_lfill,d_dummy.jpeg,e_sharpen:60,f_auto,h_450,q_auto,w_450/itemimages/60/52/6052426.jpeg", Type=""},
            };

            foreach (AssetImage i in assetsImages)
            {
                context.Images.Add(i);
            }

            context.SaveChanges();
        }
    }
}
