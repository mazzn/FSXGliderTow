using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.FlightSimulator.SimConnect;
using System.Runtime.InteropServices;

namespace FSX_Glider_Tow
{
    public partial class Form1 : Form
    {

        #region simconnect
        const int WM_USER_SIMCONNECT = 0x0402;
        SimConnect simconnect;

        enum DATA_REQUESTS
        {
            REQUEST_TOWPLANE_DATA,
            REQUEST_PLANE_LIST,
            REQUEST_PLAYER_UPDATE_AND_ATTACH,
        };

        enum EVENTS
        {
            EVENT_TOWPLANE_DETACHED,
            EVENT_TOWPLANE_REQUESTED,
        };
        enum NOTIFICATION_GROUPS
        {
            GROUP0,
        };

        enum DEFINITIONS
        {
            AirplaneData,
            AIData,
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        struct AirplaneData
        {
            // this is how you declare a fixed size string
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public String title;
            public double latitude;
            public double longitude;
            public double altitude;
            public double bank;
            public double heading;
            public double pitch;
            public Int64 desired;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        struct AIData
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public String title;
            public double latitude;
            public double longitude;
            public double altitude;
            public double bank;
            public double heading;
            public double pitch;
            public double desiredspeed;
        };

        protected override void DefWndProc(ref Message m)
        {
            if (m.Msg == WM_USER_SIMCONNECT)
            {
                if (simconnect != null)
                {
                    simconnect.ReceiveMessage();
                }
            }
            else
            {
                base.DefWndProc(ref m);
            }
        }
        #endregion

        #region objects and vars
        uint towplaneid = uint.MaxValue;
        uint playerid = uint.MaxValue;
        bool isAttached = false;
        Timer attachTick = new Timer();
        #endregion


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Tick handler
            attachTick.Tick += attachTick_Tick;
        }

        void attachTick_Tick(object sender, EventArgs e)
        {
            simconnect.RequestDataOnSimObject(DATA_REQUESTS.REQUEST_PLAYER_UPDATE_AND_ATTACH, DEFINITIONS.AirplaneData, playerid, SIMCONNECT_PERIOD.ONCE, 0, 0, 1, 0);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (simconnect == null)
            {
                try
                {
                    simconnect = new SimConnect("FSX Glidertow", this.Handle, WM_USER_SIMCONNECT, null, 0);

                    #region Handlers
                    simconnect.OnRecvSimobjectData += new SimConnect.RecvSimobjectDataEventHandler(simconnect_OnRecvSimObjectData);
                    simconnect.OnRecvSimobjectDataBytype += new SimConnect.RecvSimobjectDataBytypeEventHandler(simconnect_OnRecvSimObjectByTypeData);
                    simconnect.OnRecvEvent += new SimConnect.RecvEventEventHandler(simconnect_OnRecvEvent);
                    #endregion

                    #region Subscribe to tow request and release
                    simconnect.MapClientEventToSimEvent(EVENTS.EVENT_TOWPLANE_DETACHED, "TOW_PLANE_RELEASE");
                    simconnect.MapClientEventToSimEvent(EVENTS.EVENT_TOWPLANE_REQUESTED, "TOW_PLANE_REQUEST");
                    simconnect.AddClientEventToNotificationGroup(NOTIFICATION_GROUPS.GROUP0, EVENTS.EVENT_TOWPLANE_DETACHED, false);
                    simconnect.AddClientEventToNotificationGroup(NOTIFICATION_GROUPS.GROUP0, EVENTS.EVENT_TOWPLANE_REQUESTED, true);
                    simconnect.SetNotificationGroupPriority(NOTIFICATION_GROUPS.GROUP0, SimConnect.SIMCONNECT_GROUP_PRIORITY_HIGHEST);
                    #endregion

                    #region Get info when AI spawns
                    simconnect.OnRecvEventObjectAddremove += new SimConnect.RecvEventObjectAddremoveEventHandler(simconnect_OnRecvEventObjectAddremove);
                    simconnect.OnRecvAssignedObjectId += new SimConnect.RecvAssignedObjectIdEventHandler(simconnect_OnRecvAssignedObjectId);

                    #endregion

                    #region DataDefinition AirplaneData
                    simconnect.AddToDataDefinition(DEFINITIONS.AirplaneData, "Title", null, SIMCONNECT_DATATYPE.STRING256, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                    simconnect.AddToDataDefinition(DEFINITIONS.AirplaneData, "Plane Latitude", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                    simconnect.AddToDataDefinition(DEFINITIONS.AirplaneData, "Plane Longitude", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                    simconnect.AddToDataDefinition(DEFINITIONS.AirplaneData, "Plane Altitude", "feet", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                    simconnect.AddToDataDefinition(DEFINITIONS.AirplaneData, "PLANE BANK DEGREES", "radians", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                    simconnect.AddToDataDefinition(DEFINITIONS.AirplaneData, "PLANE HEADING DEGREES TRUE", "radians", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                    simconnect.AddToDataDefinition(DEFINITIONS.AirplaneData, "PLANE PITCH DEGREES", "radians", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                    simconnect.AddToDataDefinition(DEFINITIONS.AirplaneData, "GENERAL ENG THROTTLE LEVER POSITION:1", "percent", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                    simconnect.RegisterDataDefineStruct<AirplaneData>(DEFINITIONS.AirplaneData);
                    #endregion

                    /*
                    #region DataDefinition AIData
                    simconnect.AddToDataDefinition(DEFINITIONS.AIData, "Title", null, SIMCONNECT_DATATYPE.STRING256, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                    simconnect.AddToDataDefinition(DEFINITIONS.AIData, "Plane Latitude", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                    simconnect.AddToDataDefinition(DEFINITIONS.AIData, "Plane Longitude", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                    simconnect.AddToDataDefinition(DEFINITIONS.AIData, "Plane Altitude", "feet", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                    simconnect.AddToDataDefinition(DEFINITIONS.AIData, "PLANE BANK DEGREES", "radians", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                    simconnect.AddToDataDefinition(DEFINITIONS.AIData, "PLANE HEADING DEGREES TRUE", "radians", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                    simconnect.AddToDataDefinition(DEFINITIONS.AIData, "PLANE PITCH DEGREES", "radians", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                    simconnect.AddToDataDefinition(DEFINITIONS.AIData, "AI DESIRED SPEED ", "knots", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                    simconnect.RegisterDataDefineStruct<AIData>(DEFINITIONS.AIData);
                    #endregion
                    //*/
                    
                    #region Misc
                    button1.Text = "Disconnect from FSX";
                    button2.Enabled = true;
                    #endregion

                    //simconnect.AICreateParkedATCAircraft("Extra 300S", "ABCD", "EDDS", EVENTS.EVENT_TOWPLANE_REQUESTED);
                }
                catch
                {
                    MessageBox.Show("Could not connect to FSX");
                    button1.Text = "Connect to FSX";
                    button2.Enabled = false;
                    button3.Enabled = false;
                }
            }
            else
            { disconnect(); }
        }

        private void simconnect_OnRecvAssignedObjectId(SimConnect sender, SIMCONNECT_RECV_ASSIGNED_OBJECT_ID data)
        {
            MessageBox.Show("OnRecvAssignedObjectId: " + data.dwObjectID);
        }

        private void simconnect_OnRecvEventObjectAddremove(SimConnect sender, SIMCONNECT_RECV_EVENT_OBJECT_ADDREMOVE data)
        {
            throw new NotImplementedException();
        }

        private void simconnect_OnRecvEvent(SimConnect sender, SIMCONNECT_RECV_EVENT data)
        {
            //MessageBox.Show("EVENT");

            switch(data.uEventID)
            {
                case (uint)EVENTS.EVENT_TOWPLANE_DETACHED:
                    detach_towplane();
                    break;

                case (uint)EVENTS.EVENT_TOWPLANE_REQUESTED:
                    //button2_click(this, null);
                    //lbx_planes_attachTo.SelectedIndex = 1;
                    playerid = 1;
                    attach_towplane();
                    break;
            }
        }

        private void disconnect()
        {
            attachTick.Stop();
            simconnect = null;
            button1.Text = "Connect to FSX";
            button3.Text = "Attach towplane to player";
            button2.Enabled = false;
            button3.Enabled = false;
        }

        private void simconnect_OnRecvSimObjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            switch ((DATA_REQUESTS)data.dwRequestID)
            {
                case DATA_REQUESTS.REQUEST_PLAYER_UPDATE_AND_ATTACH:
                    AirplaneData dt = (AirplaneData)data.dwData[0];
                    dt.desired = 0;
                    simconnect.SetDataOnSimObject(DEFINITIONS.AirplaneData, towplaneid, 0, dt);
                    break;

                default:
                    MessageBox.Show("Unknown Request ID " + data.dwRequestID);
                    break;
            }
        }

        private void simconnect_OnRecvSimObjectByTypeData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            switch ((DATA_REQUESTS)data.dwRequestID)
            {
                case DATA_REQUESTS.REQUEST_PLANE_LIST:
                    AirplaneData airplanedata = (AirplaneData)data.dwData[0];
                    lbx_planes_towPlane.Items.Add((uint)data.dwObjectID + ": " + airplanedata.title);
                    lbx_planes_attachTo.Items.Add((uint)data.dwObjectID + ": " + airplanedata.title);
                    lbx_planes_towPlane.SetSelected(lbx_planes_towPlane.Items.Count-1, true);
                    button3.Enabled = true;
                    break;

                default:
                    MessageBox.Show("Unknown Request ID " + data.dwRequestID);
                    break;
            }
        }
        
        private void button2_click(object sender, EventArgs e)
        {
            lbx_planes_attachTo.Items.Clear();
            lbx_planes_towPlane.Items.Clear();
            simconnect.RequestDataOnSimObjectType(DATA_REQUESTS.REQUEST_PLANE_LIST, DEFINITIONS.AirplaneData, 0, SIMCONNECT_SIMOBJECT_TYPE.AIRCRAFT);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!isAttached)
            {
                attach_towplane();
            }
            else
            {
                detach_towplane();
            }
        }

        private void attach_towplane()
        {
            try
            {
                if (towplaneid == uint.MaxValue) towplaneid = uint.Parse(lbx_planes_towPlane.SelectedItem.ToString().Split(':')[0]);
                if (playerid == uint.MaxValue) playerid = uint.Parse(lbx_planes_attachTo.SelectedItem.ToString().Split(':')[0]);
            }
            catch { ; }

            if (towplaneid != uint.MaxValue && playerid != uint.MaxValue)
            {
                simconnect.AIReleaseControl(towplaneid, DATA_REQUESTS.REQUEST_PLANE_LIST);
                simconnect.RequestDataOnSimObject(DATA_REQUESTS.REQUEST_PLAYER_UPDATE_AND_ATTACH, DEFINITIONS.AirplaneData, playerid, SIMCONNECT_PERIOD.ONCE, 0, 0, 1, 0);

                attachTick.Interval = 6;
                attachTick.Start();

                button3.Text = "Release towplane from player";

                isAttached = true;
            }
        }

        private void detach_towplane()
        {
            simconnect.AIRemoveObject(towplaneid, DATA_REQUESTS.REQUEST_PLANE_LIST);

            AirplaneData apd = new AirplaneData();
            apd.altitude = 99999;
            apd.latitude = 0;
            apd.longitude = 0;

            simconnect.SetDataOnSimObject(DEFINITIONS.AirplaneData, towplaneid, 0, apd);
            attachTick.Stop();
            button3.Text = "Attach towplane to player";
            isAttached = false;

            towplaneid = uint.MaxValue;
            playerid = uint.MaxValue;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            disconnect();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            MessageBox.Show(@"Please set the following in your FSX.cfg file:
[SIM]
TowPlaneTitle=PLANE OF CHOICE
TowPlaneClimbPitch=-11
TowPlaneMinTurnAltitude=1000
TowPlaneTurnFrequency=120
TowPlaneTurnDeltaHeading=-179");
        }

        private void lbx_planes_attachTo_SelectedIndexChanged(object sender, EventArgs e)
        {
            playerid = uint.Parse(lbx_planes_towPlane.SelectedItem.ToString().Split(':')[0]);
        }

    }
}
