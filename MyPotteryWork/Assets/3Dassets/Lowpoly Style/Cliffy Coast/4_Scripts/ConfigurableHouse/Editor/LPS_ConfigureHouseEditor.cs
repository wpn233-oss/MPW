using System;
using UnityEngine;
using UnityEditor;
using Random = UnityEngine.Random;

/* Lowpoly Style House Configuration Script
 * (c) 2020 CH Assets
 * https://chassets.wordpress.com/
 * chunityassets@gmail.com
 * https://twitter.com/chassets
 * 
 *  Will be updated to allow for more houses in the future.
 */

namespace LPS {
    [CustomEditor(typeof(LPS_ConfigureHouse)), CanEditMultipleObjects]
    public class LPS_ConfigureHouseEditor : Editor {
        #region VARS
        public LPS_ConfigureHouse T;
        public HousePartsList Parts;

        private float _currentHeightAdjust = 0;
        private GameObject _leftIvyRef, _rightIvyRef, _leftChimneyRef, _rightChimneyRef;
        private bool _makeStatic = true;
        #endregion

        #region GUI
        public override void OnInspectorGUI() {

            T = target as LPS_ConfigureHouse;
            Parts = T.HousePartsList;

            EditorGUILayout.HelpBox("LPS House Generator 1.01 \nEasily create and randomize buildings from this pack.\n" + "Attention: Please add custom additional models or scripts beneath the \"" + LPS_ConfigureHouse.CUSTOM_OBJECTS_CONTAINER_NAME + "\" object.", MessageType.Info);
            base.DrawDefaultInspector();

            _makeStatic = EditorGUILayout.Toggle("Make objects static?", _makeStatic);
            EditorGUILayout.HelpBox("Select desired house features above and click \"update\" to generate.", MessageType.Info);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Update House")) { UpdateHouse(); }

            if (GUILayout.Button("Save to Prefab")) {
                UpdateHouse();
                string path = EditorUtility.SaveFilePanelInProject("Save as prefab", "NewHouse", "prefab", "Save as prefab");
                GameObject pf = PrefabUtility.SaveAsPrefabAsset(T.gameObject, path);
                DestroyImmediate(pf.GetComponent<LPS_ConfigureHouse>(),true);
            }
            if (GUILayout.Button("Randomize House")) {
                RandomizeHouse();
                UpdateHouse();
            }
            GUILayout.EndHorizontal();
        }
        #endregion

        private void RandomizeHouse() {
            T.BuildingType = (LPS_ConfigureHouse.HouseType)Mathf.RoundToInt(Random.Range(0, Enum.GetNames(typeof(LPS_ConfigureHouse.HouseType)).Length));
            T.RoofColor = (LPS_ConfigureHouse.HouseRoofColor)Mathf.RoundToInt(Random.Range(0, Enum.GetNames(typeof(LPS_ConfigureHouse.HouseRoofColor)).Length));

            if (Random.value <= 0.33f) { //33% chance of "full house" :)
                T.UseWoodDecorationRoof = true;
                T.CornerStones = LPS_ConfigureHouse.CornerstoneType.BottomAndTop;
                T.Chimney = LPS_ConfigureHouse.ChimneyType.Right;
                T.TopWindow = LPS_ConfigureHouse.TopWindowType.Right;
            } else {
                T.UseWoodDecorationRoof = Random.value > 0.5f;
                T.CornerStones = (LPS_ConfigureHouse.CornerstoneType)Mathf.RoundToInt(Random.Range(0, Enum.GetNames(typeof(LPS_ConfigureHouse.CornerstoneType)).Length));
                T.Chimney = (LPS_ConfigureHouse.ChimneyType)Mathf.RoundToInt(Random.Range(0, Enum.GetNames(typeof(LPS_ConfigureHouse.ChimneyType)).Length));
                T.TopWindow = (LPS_ConfigureHouse.TopWindowType)Mathf.RoundToInt(Random.Range(0, Enum.GetNames(typeof(LPS_ConfigureHouse.TopWindowType)).Length));
            }

            T.Ivy = (LPS_ConfigureHouse.IvyType)Mathf.RoundToInt(Random.Range(0, Enum.GetNames(typeof(LPS_ConfigureHouse.IvyType)).Length));
            T.PergolaLeft = (LPS_ConfigureHouse.PergolaType)Mathf.RoundToInt(Random.Range(0, Enum.GetNames(typeof(LPS_ConfigureHouse.PergolaType)).Length));
            T.PergolaRight = (LPS_ConfigureHouse.PergolaType)Mathf.RoundToInt(Random.Range(0, Enum.GetNames(typeof(LPS_ConfigureHouse.PergolaType)).Length));
        }

        #region HOUSE CREATION
        void UpdateHouse() {
            ClearHouse();

            //User added content
            bool customObjectsFound = false;
            for (int i = T.transform.childCount - 1; i >= 0; i--) {
                if (T.transform.GetChild(i).name == LPS_ConfigureHouse.CUSTOM_OBJECTS_CONTAINER_NAME) {
                    customObjectsFound = true;
                }
            }
            if (!customObjectsFound) {
                GameObject customObjectContainer = new GameObject(LPS_ConfigureHouse.CUSTOM_OBJECTS_CONTAINER_NAME);
                customObjectContainer.transform.parent = T.transform;
            }

            GameObject exteriorModel = Parts.House1_Exteriour;
            GameObject interiorModel = Parts.House1_Interour;
            GameObject exteriorOnlyModel = Parts.House1_Exteriour_Only;
            _currentHeightAdjust = 0;

            switch (T.BuildingType) {
                case LPS_ConfigureHouse.HouseType.House1:
                    exteriorModel = Parts.House1_Exteriour;
                    exteriorOnlyModel = Parts.House1_Exteriour_Only;
                    interiorModel = Parts.House1_Interour;
                    break;
                case LPS_ConfigureHouse.HouseType.House2:
                    exteriorModel = Parts.House2_Exteriour;
                    exteriorOnlyModel = Parts.House2_Exteriour_Only;
                    interiorModel = Parts.House2_Interour;
                    _currentHeightAdjust = LPS_ConfigureHouse.House_2_3_HeightReduction;
                    break;
                case LPS_ConfigureHouse.HouseType.House3:
                    exteriorModel = Parts.House3_Exteriour;
                    exteriorOnlyModel = Parts.House3_Exteriour_Only;
                    interiorModel = Parts.House3_Interour;
                    _currentHeightAdjust = LPS_ConfigureHouse.House_2_3_HeightReduction;
                    break;
                    //case LPS_ConfigureHouse.HouseType.House4:
                    //    exteriorModel = parts.House4_Exteriour;
                    //    exteriorOnlyModel = parts.House4_Exteriour_Only;
                    //    interiorModel = parts.House4_Interour;
                    //    _currentHeightAdjust = LPS_ConfigureHouse.House_2_3_HeightReduction;
                    //    break;
            }
            _currentHeightAdjust *= T.transform.localScale.y;

            //update roof
            UpdateRoof();
            UpdateBuilding(exteriorModel, exteriorOnlyModel, interiorModel);
            UpdateOptionals();
        }

        private void UpdateBuilding(GameObject exteriorModel, GameObject exteriorOnlyModel, GameObject interiorModel) {

            switch (T.Mode) {
                case LPS_ConfigureHouse.HouseMode.WithInteriour:
                    AddHousePart(exteriorModel);
                    AddHousePart(interiorModel);
                    AddDoors();
                    AddFurniture();
                    break;
                case LPS_ConfigureHouse.HouseMode.ExteriourOnly:
                    AddHousePart(exteriorOnlyModel);
                    //  t.Furniture = LPS_ConfigureHouse.FurnitureMode.None;
                    break;
            }
        }

        private void AddFurniture() {
            //if (t.FurniturePrefabOverride)
            //    AddHousePart(t.FurniturePrefabOverride);

            switch (T.BuildingType) {
                case LPS_ConfigureHouse.HouseType.House1:
                    //TODO add Furniture in next update
                    break;
                case LPS_ConfigureHouse.HouseType.House2:
                    break;
                case LPS_ConfigureHouse.HouseType.House3:
                    break;
                    //case LPS_ConfigureHouse.HouseType.House4:
                    //    break;
                    //TODO add unfinished house in next update
            }
        }

        private void AddDoors() {
            switch (T.BuildingType) {
                case LPS_ConfigureHouse.HouseType.House1:
                    SetDoorDynamic(AddHousePart(Parts.Door, LPS_ConfigureHouse.Door_Entrance_PosLeft));
                    SetDoorDynamic(AddHousePart(Parts.Door, LPS_ConfigureHouse.Door_Entrance_PosRight));
                    SetDoorDynamic(AddHousePart(Parts.Door, LPS_ConfigureHouse.Door_Interior_PosBottom));
                    SetDoorDynamic(AddHousePart(Parts.Door, LPS_ConfigureHouse.Door_Interior_PosTop));
                    break;
                case LPS_ConfigureHouse.HouseType.House2:
                    SetDoorDynamic(AddHousePart(Parts.Door, LPS_ConfigureHouse.Door_Entrance_PosLeft));
                    SetDoorDynamic(AddHousePart(Parts.Door, LPS_ConfigureHouse.Door_Entrance_PosRight));
                    SetDoorDynamic(AddHousePart(Parts.Door, LPS_ConfigureHouse.Door_Interior_PosBottom));
                    SetDoorDynamic(AddHousePart(Parts.Door, LPS_ConfigureHouse.Door_Interior_PosTop));
                    break;
                case LPS_ConfigureHouse.HouseType.House3:
                    SetDoorDynamic(AddHousePart(Parts.Door, LPS_ConfigureHouse.Door_Entrance_PosLeft));
                    SetDoorDynamic(AddHousePart(Parts.Door, LPS_ConfigureHouse.Door_Entrance_PosRight));
                    SetDoorDynamic(AddHousePart(Parts.Door, LPS_ConfigureHouse.Door_Interior_PosBottom));
                    break;
                    //case LPS_ConfigureHouse.HouseType.House4:
                    //    break;
            }
        }

        private void UpdateOptionals() {
            //TODO switch house types when 
            if (T.UseWoodDecorationRoof) AddHousePart(Parts.House1_WoodUnderRoof, true);

            //CORNERSTONES------------------------------------------------------------------------
            switch (T.CornerStones) {
                case LPS_ConfigureHouse.CornerstoneType.Bottom:
                    AddHousePart(Parts.House1_Cornerstones);
                    break;
                case LPS_ConfigureHouse.CornerstoneType.BottomAndTop:
                    AddHousePart(Parts.House1_Cornerstones);

                    if (T.BuildingType == LPS_ConfigureHouse.HouseType.House1) {
                        GameObject cs = AddHousePart(Parts.House1_Cornerstones, LPS_ConfigureHouse.Cornerstones_PosTop);
                        if (!T.UseWoodDecorationRoof)
                            cs.transform.localScale = Vector3.one;
                    } else {
                        T.CornerStones = LPS_ConfigureHouse.CornerstoneType.Bottom;
                    }
                    break;
            }

            //PERGOLA------------------------------------------------------------------------------
            switch (T.PergolaLeft) {
                case LPS_ConfigureHouse.PergolaType.Mini: AddHousePart(Parts.PergolaMini, LPS_ConfigureHouse.PergolaMini_PosLeft); break;
                case LPS_ConfigureHouse.PergolaType.Small: AddHousePart(Parts.PergolaSmall, LPS_ConfigureHouse.PergolaSmall_PosLeft); break;
                case LPS_ConfigureHouse.PergolaType.Large: AddHousePart(Parts.PergolaLarge, LPS_ConfigureHouse.PergolaLarge_PosLeft); break;
            }
            switch (T.PergolaRight) {
                case LPS_ConfigureHouse.PergolaType.Mini: AddHousePart(Parts.PergolaMini, LPS_ConfigureHouse.PergolaMini_PosRight); break;
                case LPS_ConfigureHouse.PergolaType.Small: AddHousePart(Parts.PergolaSmall, LPS_ConfigureHouse.PergolaSmall_PosRight); break;
                case LPS_ConfigureHouse.PergolaType.Large: AddHousePart(Parts.PergolaLarge, LPS_ConfigureHouse.PergolaLarge_PosRight); break;
            }

            //IVY----------------------------------------------------------------------------------
            switch (T.Ivy) {
                case LPS_ConfigureHouse.IvyType.Left:
                    _leftIvyRef = AddHousePart(Parts.IvyBottom, LPS_ConfigureHouse.Ivy_PosLeft); break;
                case LPS_ConfigureHouse.IvyType.Right:
                    _rightIvyRef = AddHousePart(Parts.IvyBottom, LPS_ConfigureHouse.Ivy_PosRight); break;
                case LPS_ConfigureHouse.IvyType.Both:
                    _leftIvyRef = AddHousePart(Parts.IvyBottom, LPS_ConfigureHouse.Ivy_PosLeft);
                    _rightIvyRef = AddHousePart(Parts.IvyBottom, LPS_ConfigureHouse.Ivy_PosRight); break;
            }
            switch (T.BuildingType) {
                case LPS_ConfigureHouse.HouseType.House2:
                    if (_leftIvyRef) _leftIvyRef.transform.localScale = new Vector3(1, 0.71f, 1);
                    if (_rightIvyRef) _rightIvyRef.transform.localScale = new Vector3(1, 0.75f, 1);
                    break;
                case LPS_ConfigureHouse.HouseType.House3:
                    if (_leftIvyRef) _leftIvyRef.transform.localScale = new Vector3(1, 0.71f, 1);
                    if (_rightIvyRef) _rightIvyRef.transform.localScale = new Vector3(1, 0.75f, 1);
                    break;
                    //case LPS_ConfigureHouse.HouseType.House4:
                    //    if (_leftIvyRef) _leftIvyRef.transform.localScale = new Vector3(1, 0.71f, 1);
                    //    if (_rightIvyRef) _rightIvyRef.transform.localScale = new Vector3(1, 0.75f, 1);
                    //    break;
            }
            _leftIvyRef = _rightIvyRef = null;

            //CHIMNEY-------------------------------------------------------------------------------
            switch (T.Chimney) {
                case LPS_ConfigureHouse.ChimneyType.Left:
                    _leftChimneyRef = AddHousePart(Parts.Chimney, LPS_ConfigureHouse.ChimneyPosition_Left_House1, true);
                    break;
                case LPS_ConfigureHouse.ChimneyType.Right:
                    _rightChimneyRef = AddHousePart(Parts.Chimney, LPS_ConfigureHouse.ChimneyPosition_Right_House1, true);
                    break;
                case LPS_ConfigureHouse.ChimneyType.Both:
                    _leftChimneyRef = AddHousePart(Parts.Chimney, LPS_ConfigureHouse.ChimneyPosition_Left_House1, true);
                    _rightChimneyRef = AddHousePart(Parts.Chimney, LPS_ConfigureHouse.ChimneyPosition_Right_House1, true);
                    break;
            }
            if (!T.ChimneySmoking) {
                if (_leftChimneyRef && _leftChimneyRef.GetComponentInChildren<ParticleSystem>()) DestroyImmediate(_leftChimneyRef.GetComponentInChildren<ParticleSystem>().gameObject);
                if (_rightChimneyRef && _rightChimneyRef.GetComponentInChildren<ParticleSystem>()) DestroyImmediate(_rightChimneyRef.GetComponentInChildren<ParticleSystem>().gameObject);
            }
            _rightChimneyRef = _leftChimneyRef = null;

        }

        private void UpdateRoof(float heightAdjust = 0) {

            GameObject SmallRoof = Parts.Small_Roof_Top_Wood;
            bool tiles = (T.RoofType == LPS_ConfigureHouse.HouseRoofType.Tiles);
            switch (T.RoofColor) {
                case LPS_ConfigureHouse.HouseRoofColor.Wood:
                    AddHousePart(tiles ? Parts.House1_Roof_Wood : Parts.House1_Roof_Planks_Wood, true);
                    SmallRoof = Parts.Small_Roof_Top_Wood;
                    break;
                case LPS_ConfigureHouse.HouseRoofColor.Red:
                    AddHousePart(tiles ? Parts.House1_Roof_Red : Parts.House1_Roof_Planks_Red, true);
                    SmallRoof = Parts.Small_Roof_Top_Red;
                    break;
                case LPS_ConfigureHouse.HouseRoofColor.Blue:
                    AddHousePart(tiles ? Parts.House1_Roof_Blue : Parts.House1_Roof_Planks_Blue, true);
                    SmallRoof = Parts.Small_Roof_Top_Blue;
                    break;
                case LPS_ConfigureHouse.HouseRoofColor.Grey:
                    AddHousePart(tiles ? Parts.House1_Roof_Grey : Parts.House1_Roof_Planks_Grey, true);
                    SmallRoof = Parts.Small_Roof_Top_Grey;
                    break;
            }

            switch (T.TopWindow) {
                case LPS_ConfigureHouse.TopWindowType.Left:
                    AddHousePart(SmallRoof, LPS_ConfigureHouse.SmallRoofTop_Left_House1, true);
                    AddHousePart(Parts.TopWindow, LPS_ConfigureHouse.TopWindowPosition_Left_House1, true);
                    break;
                case LPS_ConfigureHouse.TopWindowType.Right:
                    AddHousePart(SmallRoof, LPS_ConfigureHouse.SmallRoofTop_Right_House1, true);
                    AddHousePart(Parts.TopWindow, LPS_ConfigureHouse.TopWindowPosition_Right_House1, true);
                    break;
                case LPS_ConfigureHouse.TopWindowType.Both:
                    AddHousePart(SmallRoof, LPS_ConfigureHouse.SmallRoofTop_Left_House1, true);
                    AddHousePart(Parts.TopWindow, LPS_ConfigureHouse.TopWindowPosition_Left_House1, true);
                    AddHousePart(SmallRoof, LPS_ConfigureHouse.SmallRoofTop_Right_House1, true);
                    AddHousePart(Parts.TopWindow, LPS_ConfigureHouse.TopWindowPosition_Right_House1, true);
                    break;
            }
        }
        #endregion

        #region UTILITY

        private GameObject FixName(GameObject g) {
            if (g) g.name = g.name.Replace("(Clone)", "");
            return g;
        }
        public GameObject AddHousePart(GameObject g, bool UseHeightAdjust = false, Transform parent = null) {
            if (parent == null) parent = T.transform;
            GameObject p = FixName(Instantiate(g, parent));

            if (UseHeightAdjust) {
                p.transform.Translate(Vector3.up * _currentHeightAdjust);
            }
            if (_makeStatic) {
                p.isStatic = true;
            }
            return p;
        }
        public GameObject AddHousePart(GameObject g, Vector3 pos, Vector3? euler = null, bool UseHeightAdjust = false, Transform parent = null) {
            if (euler == null) euler = Vector3.zero;
            if (parent == null) parent = T.transform;

            GameObject p = FixName(Instantiate(g, parent));
            p.transform.localPosition = pos;
            p.transform.localEulerAngles = (Vector3)euler;
            if (UseHeightAdjust) {
                p.transform.Translate(Vector3.up * _currentHeightAdjust);
            }
            if (_makeStatic) {
                p.isStatic = true;
            }
            return p;
        }
        public GameObject AddHousePart(GameObject g, HousePartPosition hpp, bool UseHeightAdjust = false, Transform parent = null) {
            if (parent == null) parent = T.transform;

            GameObject p = FixName(Instantiate(g, parent));
            p.transform.localPosition = hpp.Position;
            p.transform.localEulerAngles = hpp.Euler;
            p.transform.localScale = hpp.Scale;
            if (UseHeightAdjust) {
                p.transform.Translate(Vector3.up * _currentHeightAdjust);
            }
            if (_makeStatic) {
                p.isStatic = true;
            }
            return p;
        }
        void ClearHouse() {
            T.RemoveAllChildren();
            _currentHeightAdjust = 0;
        }

        /// <summary>
        /// Use this to keep the prefab connection. Currently not used, so that changes in the building aren't accidentally applied to individual prefabs (they should always remain the same, for this to work flawlessly).
        ///  To update after prefab changes, just press update in the script in inspector.
        /// </summary>
        private GameObject PrefabUtilInstantiateAndParent(GameObject g, Transform parent = null) {
            if (parent == null) parent = T.transform;
            GameObject prefab = PrefabUtility.InstantiatePrefab(g) as GameObject;
            Vector3 p = prefab.transform.localPosition;
            Quaternion r = prefab.transform.localRotation;
            prefab.transform.parent = parent;
            prefab.transform.localPosition = p;
            prefab.transform.localRotation = r;
            prefab.transform.localScale = parent.localScale;
            return prefab;
        }

        public void SetStaticEditorFlag(GameObject go, StaticEditorFlags flag, bool enableFlag) {
            var currentFlags = GameObjectUtility.GetStaticEditorFlags(go);
            if (enableFlag) {
                currentFlags |= flag;
            } else {
                currentFlags &= ~flag;
            }
            GameObjectUtility.SetStaticEditorFlags(go, currentFlags);
        }

        public GameObject SetDoorDynamic(GameObject g) {
            if (_makeStatic) {
                SetStaticEditorFlag(g, StaticEditorFlags.BatchingStatic, false);
                SetStaticEditorFlag(g, StaticEditorFlags.NavigationStatic, false);
            }
            return g;
        }
        #endregion
    }
}
