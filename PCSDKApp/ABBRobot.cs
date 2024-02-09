using ABB.Robotics.Controllers;
using ABB.Robotics.Controllers.RapidDomain;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Windows;

namespace Painting
{
    internal class ABBRobot
    {
        private RapidData rd_vector = null;
        private Pos pos_vector;
        public Point vector;

        private RapidData rd_flag = null;
        private Num flag;

        private int TO_STOP = -1;
        private int TO_START = 1;

        public Controller SelectedController { get; set;  }


        public ABBRobot()
        {
            pos_vector = new Pos();
            vector = new Point();
        }

        public void Connect(ControllerInfo controllerInfo)
        {
            if (controllerInfo is null) return;

            SelectedController = Controller.Connect(controllerInfo, ConnectionType.Standalone);
            SelectedController.Logon(UserInfo.DefaultUser);

            Init();
        }

        public void Init()
        {
            var taskRobot = SelectedController.Rapid.GetTask("T_ROB1");
            if (taskRobot != null)
            {
                rd_flag = taskRobot.GetRapidData("Module1", "flag");
                if (rd_flag.Value is Num)
                {
                    flag = (Num)rd_flag.Value;
                }

                rd_vector = taskRobot.GetRapidData("Module1", "vector");
                if (rd_vector.Value is Pos)
                {
                    pos_vector = (Pos)rd_vector.Value;
                }
            }
        }

        public void StartExec()
        {
            if (SelectedController == null) return;
            
            try
            {
                if (SelectedController.OperatingMode == ControllerOperatingMode.Auto && SelectedController.State == ControllerState.MotorsOn)
                {
                    using (Mastership m = Mastership.Request(SelectedController))
                    {
                        StartResult result = SelectedController.Rapid.Start(true);
                    }
                }
                else
                {
                    MessageBox.Show("Automatic mode is required to start execution from a remote client.");
                }
            }
            catch (System.InvalidOperationException ex)
            {
                MessageBox.Show("Mastership is held by another client." + ex.Message);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Unexpected error occurred: " + ex.Message);
            }
        }
        public void StopExec()
        {
            if (SelectedController == null) return;

            try
            {
                if (SelectedController.OperatingMode == ControllerOperatingMode.Auto && SelectedController.State == ControllerState.MotorsOn)
                {
                    using (Mastership m = Mastership.Request(SelectedController))
                    {
                        SelectedController.Rapid.Stop(StopMode.Immediate);
                    }
                }
                else
                {
                    MessageBox.Show("Automatic mode is required to start execution from a remote client.");
                }
            }
            catch (System.InvalidOperationException ex)
            {
                MessageBox.Show("Mastership is held by another client." + ex.Message);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Unexpected error occurred: " + ex.Message);
            }
        }

        public void Move()
        {
            if (SelectedController is null) return;
            
            SetDirection();
            StartProc();
        }

        public void StopProc()
        {
            flag.FillFromString2(TO_STOP.ToString());
            using (Mastership m = Mastership.Request(SelectedController))
            {
                rd_flag.Value = flag;
            }
        }
        public void StartProc()
        {
            flag.FillFromString2(TO_START.ToString());
            using (Mastership m = Mastership.Request(SelectedController))
            {
                rd_flag.Value = flag;
            }
        }


        public void SetDirection()
        {
            pos_vector.FillFromString2("[" + (int)vector.X + "," + (int)vector.Y + ", 0]"); // Z = 0
            using (Mastership m = Mastership.Request(SelectedController))
            {
                rd_vector.Value = pos_vector;
            }
        }
    }
}
