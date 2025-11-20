using UnityEngine;

/* Lowpoly Style House Configuration Script
 * (c) 2020 CH Assets
 * https://chassets.wordpress.com/
 * chunityassets@gmail.com
 * https://twitter.com/chassets
 * 
 *  Will be updated to allow for more houses in the future.
 */

namespace LPS {
    [DisallowMultipleComponent, ExecuteInEditMode, SelectionBase]
    public class LPS_ConfigureHouse : MonoBehaviour {
        #region READONLY HOUSE PARTS POSITIONS
        public static readonly HousePartPosition ChimneyPosition_Right_House1 = new HousePartPosition(-0.19f, 8.912f, 1.34f);
        public static readonly HousePartPosition ChimneyPosition_Left_House1 = new HousePartPosition(-0.19f, 8.912f, -2.548f, 0, 180, 0);

        public static readonly HousePartPosition TopWindowPosition_Right_House1 = new HousePartPosition(-3.9253f, 8.8848f, 1.56f);
        public static readonly HousePartPosition TopWindowPosition_Left_House1 = new HousePartPosition(-3.9615f, 8.9468f, -2.44792f, 0, 180, 0, 1, 1, 0.76f);

        public static readonly HousePartPosition SmallRoofTop_Right_House1 = new HousePartPosition(-3.9434f, 10.648f, -0.72632f);
        public static readonly HousePartPosition SmallRoofTop_Left_House1 = new HousePartPosition(-3.9434f, 10.648f, -0.72632f, 0, 180f, 0, 1, 1, 0.76f);

        public static readonly HousePartPosition PergolaMini_PosRight = new HousePartPosition(4.260617f, 3.168064f, 1.883266f);
        public static readonly HousePartPosition PergolaMini_PosLeft = new HousePartPosition(-3.868f, 3.168064f, -3.089f, 0, 180, 0);

        public static readonly HousePartPosition PergolaSmall_PosRight = new HousePartPosition(4.169f, -0.15f, 1.6f, 0, 90f, 0, 1, 1, 0.888f);
        public static readonly HousePartPosition PergolaSmall_PosLeft = new HousePartPosition(-3.67f, -0.15f, -2.84f, 0, -90f, 0);

        public static readonly HousePartPosition PergolaLarge_PosRight = new HousePartPosition(3.138f, -0.15f, 1.8f, 0, 90f, 0, 1, 1, 0.831f);
        public static readonly HousePartPosition PergolaLarge_PosLeft = new HousePartPosition(-1.8358f, -0.15f, -2.84f, 0, -90f, 0);

        public static readonly HousePartPosition Door_Entrance_PosRight = new HousePartPosition(3.356939f, 2.318817f, 1.935775f);
        public static readonly HousePartPosition Door_Entrance_PosLeft = new HousePartPosition(-3.0287f, 2.3188f, -3.1801f, 0, 180f, 0);
        public static readonly HousePartPosition Door_Interior_PosBottom = new HousePartPosition(-0.3245992f, 2.426637f, -1.169022f, 0, 90f, 0);
        public static readonly HousePartPosition Door_Interior_PosTop = new HousePartPosition(-0.3245992f, 5.120971f, -1.169022f, 0, 90f, 0);

        public static readonly HousePartPosition Cornerstones_PosTop = new HousePartPosition(-0f, 3.4f, -0f, 0, 0, 0, 1, 0.7431853f, 1f);

        public static readonly HousePartPosition Ivy_PosRight = new HousePartPosition(-0.285f, 0.616f, 3.59f);
        public static readonly HousePartPosition Ivy_PosLeft = new HousePartPosition(-1.241f, 0.615f, -3.13f, 0, 90, 0);

        public const float House_2_3_HeightReduction = -2.0183f;
        public const string CUSTOM_OBJECTS_CONTAINER_NAME = "CustomObjectContainer";
        #endregion

        #region PUBLIC
        [Header("Connection")]
        public HousePartsList HousePartsList;

        public enum HouseType { House1, House2, House3 /*, House4*/ }
        [Header("House Settings")]
        public HouseType BuildingType = HouseType.House1;
        public enum HouseMode { WithInteriour, ExteriourOnly }
        public HouseMode Mode = HouseMode.ExteriourOnly;

        public enum HouseRoofColor { Wood, Red, Blue, Grey }
        [Header("Roof")]
        public HouseRoofColor RoofColor = HouseRoofColor.Red;
        public enum HouseRoofType { Tiles, Planks }
        public HouseRoofType RoofType = HouseRoofType.Tiles;

        [Header("Decoration")]
        public bool UseWoodDecorationRoof = true;
        public enum ChimneyType { None, Left, Right, Both }
        public ChimneyType Chimney = ChimneyType.Right;
        public bool ChimneySmoking = false;
        public enum CornerstoneType { None, Bottom, BottomAndTop }
        public CornerstoneType CornerStones = CornerstoneType.Bottom;
        public enum TopWindowType { None, Left, Right, Both }
        public TopWindowType TopWindow = TopWindowType.Right;
        public enum PergolaType { None, Mini, Small, Large }
        public PergolaType PergolaLeft = PergolaType.None;
        public PergolaType PergolaRight = PergolaType.None;
        public enum IvyType { None, Left, Right, Both }
        public IvyType Ivy = IvyType.None;

        /*
        public enum FurnitureMode { None, Nice, Normal, Cheap }
        [Header("Furniture (interior mode only)")]
        public FurnitureMode Furniture = FurnitureMode.None; 
        public GameObject FurniturePrefabOverride;
        */
        #endregion

        public void RemoveAllChildren() {
            for (int i = transform.childCount - 1; i >= 0; i--) {
                if (transform.GetChild(i).name != CUSTOM_OBJECTS_CONTAINER_NAME)
                    DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }

        void Start() {
            if (Application.isPlaying) {
                Destroy(this);
            }
        }
    }

    public struct HousePartPosition {
        public HousePartPosition(Vector3 pos, Vector3 euler) {
            Position = pos;
            Euler = euler;
            Scale = new Vector3(1, 1, 1);
        }
        public HousePartPosition(Vector3 pos) {
            Position = pos;
            Euler = new Vector3(0, 0, 0);
            Scale = new Vector3(1, 1, 1);
        }
        public HousePartPosition(float x, float y, float z) {
            Position = new Vector3(x, y, z); ;
            Euler = new Vector3(0, 0, 0);
            Scale = new Vector3(1, 1, 1);
        }
        public HousePartPosition(float x, float y, float z, float eulerX, float eulerY, float eulerZ) {
            Position = new Vector3(x, y, z); ;
            Euler = new Vector3(eulerX, eulerY, eulerZ);
            Scale = new Vector3(1, 1, 1);
        }
        public HousePartPosition(float x, float y, float z, float eulerX, float eulerY, float eulerZ, float sX, float sY, float sZ) {
            Position = new Vector3(x, y, z); ;
            Euler = new Vector3(eulerX, eulerY, eulerZ);
            Scale = new Vector3(sX, sY, sZ);
        }
        public Vector3 Position;
        public Vector3 Euler;
        public Vector3 Scale;
    }
}

