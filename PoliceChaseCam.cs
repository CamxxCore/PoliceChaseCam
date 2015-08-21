using System;
using System.Collections.Generic;
using System.Linq;
using GTA;
using GTA.Native;
using GTA.Math;

namespace GTAV_PoliceChaseCam
{
    public class PoliceChaseCam : Script
    {
        Camera mainCamera;

        public PoliceChaseCam()
        {
            this.Tick += OnTick;
        }

        bool notifyHeli, notifyVehicle;

        int mViewModeCounter = 0;

        public void OnTick(object sender, EventArgs e)
        {
            var veh = FindNearbyPoliceVehicle(VehicleType.Car);
            if (veh != null && veh.Handle != 0 && !notifyVehicle)
            {
                UI.Notify("Vehicle Cam Available.");
                notifyVehicle = true;
            }

            else if (veh == null || veh.Handle == 0)
                notifyVehicle = false;

            veh = FindNearbyPoliceVehicle(VehicleType.Helicopter);

            if (veh != null && veh.Handle != 0 && !notifyHeli)
            {
                UI.Notify("Helicopter Cam Available.");
                notifyHeli = true;
            }

            else if (veh == null || veh.Handle == 0)
                notifyHeli = false;

            if (Game.Player.Character.IsOnFoot && Function.Call<int>(Hash.GET_FOLLOW_PED_CAM_VIEW_MODE) == 2 || Game.Player.Character.IsInVehicle() && Function.Call<int>(Hash.GET_FOLLOW_VEHICLE_CAM_VIEW_MODE) == 2)
            {
                Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, 0, true);

                if (Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED, 0, 0))
                {
                    var player = Game.Player.Character;

                    switch (mViewModeCounter)
                    {
                        case 0:
                            var policeVeh = FindNearbyPoliceVehicle(VehicleType.Car);
                            if (policeVeh != null && policeVeh.Handle != 0)
                            {
                                mainCamera = World.CreateCamera(policeVeh.Position, policeVeh.Rotation, 50f);
                                mainCamera.AttachTo(policeVeh, new Vector3(0, 1f, 0.5f));
                                mainCamera.PointAt(player);
                                World.RenderingCamera = mainCamera;
                                mViewModeCounter++;

                            }
                            else
                            {
                                mViewModeCounter++;
                                goto case1;
                            }
                            break;
                        case 1:
                        case1:
                            var policeHeli = FindNearbyPoliceVehicle(VehicleType.Helicopter);
                            if (policeHeli != null && policeHeli.Handle != 0)
                            {
                                mainCamera = World.CreateCamera(policeHeli.Position, policeHeli.Rotation, 50f);
                                mainCamera.AttachTo(policeHeli, new Vector3(0, 0, -1.4f));
                                mainCamera.PointAt(player);
                                World.RenderingCamera = mainCamera;
                                mViewModeCounter++;
                            }

                            else
                            {
                                ExitCustomCameraView();
                                mViewModeCounter = 0;
                            }
                            break;

                        default:
                            ExitCustomCameraView();
                            mViewModeCounter = 0;
                            break;
                    }
                }
            }
        }

        private Vehicle FindNearbyPoliceVehicle(VehicleType vehicleType)
        {
            var player = Game.Player.Character;
            var nearbyVehicles = World.GetNearbyVehicles(player, 12000f);
            switch (vehicleType)
            {
                case VehicleType.Car:
                    var hashes = new List<VehicleHash>{
                                VehicleHash.Police,
                                VehicleHash.Police2,
                                VehicleHash.Police3,
                                VehicleHash.Police4,
                                VehicleHash.Policeb,
                                VehicleHash.PoliceT,
                                VehicleHash.Sheriff,
                                VehicleHash.Sheriff2,
                                VehicleHash.FBI,
                                VehicleHash.FBI2,
                                VehicleHash.PoliceOld1,
                                VehicleHash.PoliceOld2,
                                VehicleHash.Riot,
                                VehicleHash.Pranger,
                            };
                    return nearbyVehicles
                          .Where(x => hashes.Any(v => v == x.Model) && !x.IsSeatFree(VehicleSeat.Driver))
                          .OrderBy(x => x.Position.DistanceTo(player.Position))
                          .FirstOrDefault();
                case VehicleType.Helicopter:
                    return nearbyVehicles
                                    .Where(x => x.Handle != 0 && x.Model == VehicleHash.Polmav)
                                    .OrderBy(x => x.Position.DistanceTo(player.Position))
                                    .FirstOrDefault();

                default: throw new ArgumentException("VehicleType: Not a valid type.");
            }
        }

        private void ExitCustomCameraView()
        {
            if (Game.Player.Character.IsInVehicle())
            {
                int mode = Function.Call<int>(Hash.GET_FOLLOW_VEHICLE_CAM_VIEW_MODE);
                mode = mode < 4 ? mode + 1 : 0;
                Function.Call(Hash.SET_FOLLOW_VEHICLE_CAM_VIEW_MODE, mode);
            }

            else if (Game.Player.Character.IsOnFoot)
            {
                int mode = Function.Call<int>(Hash.GET_FOLLOW_PED_CAM_VIEW_MODE);
                mode = mode < 4 ? mode + 1 : 0;
                Function.Call(Hash.SET_FOLLOW_PED_CAM_VIEW_MODE, mode);
            }

            Script.Wait(100);
            World.RenderingCamera = null;
            Function.Call(Hash.DESTROY_ALL_CAMS, true);
        }

        public enum VehicleType
        {
            Car = 0,
            Helicopter = 1
        }

        protected override void Dispose(bool A_0)
        {
            World.RenderingCamera = null;
            base.Dispose(A_0);
        }
    }
}
