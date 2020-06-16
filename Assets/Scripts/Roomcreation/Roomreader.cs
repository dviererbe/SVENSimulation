using Assets.Scripts.Roomcreation;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using System.Globalization;

namespace Assets.Scripts
{
    class RoomReader
    {
        private static string[] MASSEINHEIT = new string[]
        {
            "Millimeter",
            "Centimeter",
            "Dezimeter",
            "Meter"
        };

        private static string PATHWALL = "/Roomplan/Walls/Wall";
        private static string PATHWINDOW = "/Roomplan/Walls/Window";
        private static string PATHDOOR = "/Roomplan/Walls/Door";
        private static string PATHCHAIR = "/Roomplan/Furniture/Chair";
        private static string PATHTABLE = "/Roomplan/Furniture/Table";
        private static string PATHHEATER = "/Roomplan/Furniture/Heater";
        private static string PATHCLOSET = "/Roomplan/Furniture/Closet";

        private static string PATHROOMHEIGTH = "/Roomplan/Room/Height";
        private static string PATHROOMWIDTH = "/Roomplan/Room/Width";
        private static string PATHROOMWALLTHICKNESS = "/Roomplan/Room/WallThickness";
        private static string PATHROOMTHERMALPIXELSIZE = "/Roomplan/Room/ThermalPixelSize";

        private static string PATHSCALING = "/Roomplan/Room/Masseinheit";
        private static string PATHSTARTHEIGTH = "./StartPosition/Height";
        private static string PATHSTARTWIDTH = "./StartPosition/Width";
        private static string PATHENDHEIGTH = "./EndPosition/Height";
        private static string PATHENDWIDTH = "./EndPosition/Width";
        private static string PATHHEIGHT = "./Position/Height";
        private static string PATHWIDTH = "./Position/Width";
        private static string PATHROTATION= "./Rotation";
        private static string PATHSIZEHEIGHT= "./Size/Height";
        private static string PATHSIZEWIDTH= "./Size/Width";
        private static string PATHTYPE= "./Type";

        private string _xmlPath;

        public RoomReader(string xmlPath)
        {
            _xmlPath = xmlPath;
        }

        public RoomObjects[] ReadRoom(out RoomObjects.RoomElement[,] walls, out float scaling)
        {
            Debug.Log("Enter");

            scaling = 1.0f;

            //StreamReader str = new StreamReader(_xmlPath);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(_xmlPath);

            XmlNode root = xmlDoc.DocumentElement;

            Debug.Log("Load File");

            #region Roomsize
            {
                //read Roomsize
                XmlNode heightNode = root.SelectSingleNode(PATHROOMHEIGTH);
                OptionsManager.RoomHeight = int.Parse(heightNode.InnerText);

                XmlNode widthNode = root.SelectSingleNode(PATHROOMWIDTH);
                OptionsManager.RoomWidth = int.Parse(widthNode.InnerText);

                XmlNode wallThickness = root.SelectSingleNode(PATHROOMWALLTHICKNESS);
                OptionsManager.WallThickness = float.Parse(wallThickness.InnerText);

                XmlNode thermalPixelSize = root.SelectSingleNode(PATHROOMTHERMALPIXELSIZE);
                OptionsManager.ThermalPixelSize = float.Parse(thermalPixelSize.InnerText);

                XmlNode scalingNode = root.SelectSingleNode(PATHSCALING);
                if(scalingNode.InnerText.Equals(MASSEINHEIT[0]))
                {
                    scaling = 1000f;
                }
                else if(scalingNode.InnerText.Equals(MASSEINHEIT[1]))
                {
                    scaling = 100f;
                }
                else if(scalingNode.InnerText.Equals(MASSEINHEIT[2]))
                {
                    scaling = 10f;
                }
                else
                {
                    scaling = 1f;
                }

            }
            #endregion

            #region Walls
            walls = new RoomObjects.RoomElement[(int)OptionsManager.RoomWidth, (int)OptionsManager.RoomHeight];

            XmlNodeList wallsNodeList = xmlDoc.SelectNodes(PATHWALL);
            walls = readWallElements(walls, wallsNodeList, RoomObjects.RoomElement.WALL);
            #endregion

            List<RoomObjects> roomobjects = new List<RoomObjects>();

            #region Windows and Doors

            XmlNodeList windows = root.SelectNodes(PATHWINDOW);
            walls = readWallElements(walls, windows, RoomObjects.RoomElement.WINDOW);

            XmlNodeList doors = root.SelectNodes(PATHDOOR);
            walls = readWallElements(walls, doors, RoomObjects.RoomElement.DOOR);

            #endregion

            #region furniture

            XmlNodeList chairs = root.SelectNodes(PATHCHAIR);
            roomobjects = readRoomobjects(roomobjects, chairs, RoomObjects.RoomElement.CHAIR);

            XmlNodeList tables = root.SelectNodes(PATHTABLE);
            roomobjects = readRoomobjects(roomobjects, tables, RoomObjects.RoomElement.TABLE);

            XmlNodeList heater = root.SelectNodes(PATHHEATER);
            roomobjects = readRoomobjects(roomobjects, heater, RoomObjects.RoomElement.HEATER);

            XmlNodeList closet = root.SelectNodes(PATHCLOSET);
            roomobjects = readRoomobjects(roomobjects, closet, RoomObjects.RoomElement.CLOSET);

            #endregion
            return roomobjects.Count == 0 ? new RoomObjects[0] : roomobjects.ToArray();

        }

        private RoomObjects.RoomElement[,] readWallElements(RoomObjects.RoomElement[,] walls, XmlNodeList elementList, RoomObjects.RoomElement type)
        {
            if (elementList != null)
            {
                foreach (XmlNode wallElement in elementList)
                {
                    XmlNode posValue = wallElement.SelectSingleNode(PATHSTARTHEIGTH);
                    int startheight = int.Parse(posValue.InnerText);

                    posValue = wallElement.SelectSingleNode(PATHSTARTWIDTH);
                    int startwidth = int.Parse(posValue.InnerText);

                    posValue = wallElement.SelectSingleNode(PATHENDHEIGTH);
                    int endheight = int.Parse(posValue.InnerText);

                    posValue = wallElement.SelectSingleNode(PATHENDWIDTH);
                    int endwidth = int.Parse(posValue.InnerText);


                    Debug.Log("SHeigth: " + startheight);
                    Debug.Log("SWidth: " + startwidth);
                    Debug.Log("EHeigth: " + endheight);
                    Debug.Log("EWidth: " + endwidth);

                    //darstellung von geraden wänden nur möglich
                    if (endheight == startheight)
                    {
                        //Tauschen der Psozionsdaten falls falsch eingetragen
                        if (startwidth > endwidth)
                        {
                            int zw = startwidth;
                            startwidth = endwidth;
                            endwidth = zw;
                        }

                        for (int i = startwidth; i <= endwidth; i++)
                        {
                            walls[i, startheight] = type;
                        }
                    }
                    else if (startwidth == endwidth)
                    {
                        //Tauschen der Psozionsdaten falls falsch eingetragen
                        if (startheight > endheight)
                        {
                            int zw = startwidth;
                            startwidth = endwidth;
                            endwidth = zw;
                        }

                        for (int i = startheight; i <= endheight; i++)
                        {
                            walls[startwidth, i] = type;
                        }
                    }
                    else
                    {
                        //TODO schräge wände
                    }
                }
            }
            return walls;
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
