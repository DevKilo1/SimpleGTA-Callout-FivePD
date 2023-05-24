using CitizenFX.Core;
using FivePD.API;
using FivePD.API.Utils;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GrandTheftAuto
{
    [Guid("79ACCFEA-62CE-46A6-BE5D-60CB046536C5")]
    [CalloutProperties("Grand Theft Auto","DevKilo","1.0")]
    public class GTA : Callout
    {
        private readonly Random rnd = new Random();
        private Ped suspect;
        private Vehicle vehicle;
        private Blip blip;
        private bool endedEarly = true;
        private bool pursuitActive = false;

        public GTA()
        {
            int distance = rnd.Next(200, 750);
            float offsetX = rnd.Next(-1 * distance, distance);
            float offsetY = rnd.Next(-1 * distance, distance);
            
            InitInfo(World.GetNextPositionOnStreet(Game.PlayerPed.GetOffsetPosition(new Vector3(offsetX, offsetY, 0))));
          
            ShortName = "Grand Theft Auto";
            CalloutDescription = "A stolen car was reportedly seen. Go get him!";
            ResponseCode = 3;
            StartDistance = 50f;

            Tick += async () =>
            {
                if (!pursuitActive && blip != null && suspect != null && Location != null)
                {
                    Location = suspect.Position;
                }
                    
            };
        }

        public override async Task OnAccept()
        {
            SpawnSuspects();
        }
        private async Task SpawnSuspects()
        {
            vehicle = await SpawnVehicle(RandomUtils.GetRandomVehicle(), Location);
            suspect = await SpawnPed(RandomUtils.GetRandomPed(), Location.ApplyOffset(new(0f, 2f, 0f)));
            suspect.SetIntoVehicle(vehicle, VehicleSeat.Driver);
            suspect.Task.CruiseWithVehicle(vehicle, 20f, 447);
            blip = suspect.AttachBlip();
            blip.ShowRoute = true;
        }
        public override void OnStart(Ped closest)
        {
            pursuitActive = true;
            blip.ShowRoute = false;
            //blip.Delete();
            //blip = null;
            Pursuit.RegisterPursuit(suspect).ActivatePursuit();//.Init(true, 100f, 100f, true);
            Utilities.ExcludeVehicleFromTrafficStop(vehicle.NetworkId, true);
            suspect.IsPersistent = true;
            vehicle.IsPersistent = true;
            suspect.AlwaysKeepTask = true;
            suspect.BlockPermanentEvents = true;
            endedEarly = false;
            Tick += async () =>
            {
                if (suspect != null)
                {
                    if (endedEarly && suspect.IsCuffed || endedEarly && suspect.IsDead)
                    {
                        endedEarly = false;
                    }
                }
            };
        }
        public override void OnCancelBefore()
        {
            blip?.Delete();
            if (endedEarly)
            {
                vehicle?.Delete();
                suspect?.Delete();
            }
        }
    }
}