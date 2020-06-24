//#define DEBUG_FILEREADER

using Assets.Scripts.Roomcreation;
using System.Collections.Generic;
using System.Xml;
using System.Globalization;

namespace Assets.Scripts
{
    class RoomReader
    {
        private const string PATHWINDOW = "/Roomplan/Walls/Window";
        private const string PATHDOOR = "/Roomplan/Walls/Door";
        private const string PATHCHAIR = "/Roomplan/Furniture/Chair";
        private const string PATHTABLE = "/Roomplan/Furniture/Table";
        private const string PATHHEATER = "/Roomplan/Furniture/Heater";
        private const string PATHCLOSET = "/Roomplan/Furniture/Closet";

        private const string PATHROOMHEIGTH = "/Roomplan/Room/Height";
        private const string PATHROOMWIDTH = "/Roomplan/Room/Width";
        private const string PATHROOMWALLTHICKNESS = "/Roomplan/Room/WallThickness";
        private const string PATHROOMTHERMALPIXELSIZE = "/Roomplan/Room/ThermalPixelSize";

        private const string PATHHEIGHT = "./Position/Height";
        private const string PATHWIDTH = "./Position/Width";
        private const string PATHROTATION= "./Rotation";
        private const string PATHSIZEHEIGHT= "./Size/Height";
        private const string PATHSIZEWIDTH= "./Size/Width";
        private const string PATHTYPE= "./Type";

        private string _xmlPath;

        public RoomReader(string xmlPath)
        {
            _xmlPath = xmlPath;
        }

        public RoomObjects[] ReadRoom()
        {

            #if DEBUG_FILEREADER
                Debug.Log("Enter");
            #endif

            //StreamReader str = new StreamReader(_xmlPath);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(_xmlPath);

            XmlNode xmlRootNode = xmlDoc.DocumentElement;

            #if DEBUG_FILEREADER
                Debug.Log("Load File");
            #endif
            #region Room Options
            {
                //read Roomsize
                XmlNode heightNode = xmlRootNode.SelectSingleNode(PATHROOMHEIGTH);
                OptionsManager.RoomHeight = int.Parse(heightNode.InnerText);

                XmlNode widthNode = xmlRootNode.SelectSingleNode(PATHROOMWIDTH);
                OptionsManager.RoomWidth = int.Parse(widthNode.InnerText);

                XmlNode wallThickness = xmlRootNode.SelectSingleNode(PATHROOMWALLTHICKNESS);
                //CultureInfo.InvariantCulture wird benötigt, damit Zahlen, wie 0.5, nicht als 5 gelesen werden!
                OptionsManager.WallThickness = float.Parse(wallThickness.InnerText, CultureInfo.InvariantCulture);

                XmlNode thermalPixelSize = xmlRootNode.SelectSingleNode(PATHROOMTHERMALPIXELSIZE);
                OptionsManager.ThermalPixelSize = float.Parse(thermalPixelSize.InnerText, CultureInfo.InvariantCulture);
            }
            #endregion

            List<RoomObjects> roomobjects = new List<RoomObjects>();

            #region Windows and Doors

            XmlNodeList windows = xmlRootNode.SelectNodes(PATHWINDOW);
            roomobjects = readRoomobjects(roomobjects, windows, RoomObjects.RoomElement.WINDOW);

            XmlNodeList doors = xmlRootNode.SelectNodes(PATHDOOR);
            roomobjects = readRoomobjects(roomobjects, doors, RoomObjects.RoomElement.DOOR);

            #endregion

            #region furniture

            XmlNodeList chairs = xmlRootNode.SelectNodes(PATHCHAIR);
            roomobjects = readRoomobjects(roomobjects, chairs, RoomObjects.RoomElement.CHAIR);

            XmlNodeList tables = xmlRootNode.SelectNodes(PATHTABLE);
            roomobjects = readRoomobjects(roomobjects, tables, RoomObjects.RoomElement.TABLE);

            XmlNodeList heater = xmlRootNode.SelectNodes(PATHHEATER);
            roomobjects = readRoomobjects(roomobjects, heater, RoomObjects.RoomElement.HEATER);

            XmlNodeList closet = xmlRootNode.SelectNodes(PATHCLOSET);
            roomobjects = readRoomobjects(roomobjects, closet, RoomObjects.RoomElement.CLOSET);

            #endregion

            return roomobjects.Count == 0 ? new RoomObjects[0] : roomobjects.ToArray();

        }

        
        private List<RoomObjects> readRoomobjects(List<RoomObjects> roomobjects, XmlNodeList elementList, RoomObjects.RoomElement type)
        {
            if(elementList != null)
            {
                foreach(XmlNode element in elementList)
                {
                    RoomObjects newobject = new RoomObjects();
                    newobject.Element = type;

                    XmlNode pos = element.SelectSingleNode(PATHHEIGHT);
                    newobject.PosX = float.Parse(pos.InnerText, CultureInfo.InvariantCulture);

                    pos = element.SelectSingleNode(PATHWIDTH);
                    newobject.PosY = float.Parse(pos.InnerText, CultureInfo.InvariantCulture);

                    pos = element.SelectSingleNode(PATHROTATION);
                    newobject.Rotation = float.Parse(pos.InnerText);

                    pos = element.SelectSingleNode(PATHTYPE);
                    newobject.Type = pos.InnerText;

                    pos = element.SelectSingleNode(PATHSIZEHEIGHT);
                    newobject.Sizeheight = float.Parse(pos.InnerText, CultureInfo.InvariantCulture);

                    pos = element.SelectSingleNode(PATHSIZEWIDTH);
                    newobject.Sizewidth = float.Parse(pos.InnerText, CultureInfo.InvariantCulture);
                    
                    roomobjects.Add(newobject);
                }
            }

            return roomobjects;
        }
    }
}
