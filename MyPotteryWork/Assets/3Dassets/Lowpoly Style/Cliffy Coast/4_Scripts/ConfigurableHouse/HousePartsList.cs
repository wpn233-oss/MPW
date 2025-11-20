using UnityEngine;

/* Lowpoly Style House Configuration Script
 * (c) 2020 CH Assets
 * https://chassets.wordpress.com/
 * chunityassets@gmail.com
 * https://twitter.com/chassets
 * 
 * Will be updated to allow for more houses in the future.
 */

[CreateAssetMenu(fileName = "HousePartsList", menuName = "ScriptableObjects/LowpolyStyle/HousePartsList", order = 1)]
public class HousePartsList : ScriptableObject {

    [Header("House1")]
    public GameObject House1_Exteriour;
    //public GameObject House1_Exteriour_RoundWindows;
    public GameObject House1_Exteriour_Only;
    public GameObject House1_Interour;
    //public GameObject House1_Interour_RoundWindows;

    /* [Header("House1 - Windows")]
     public GameObject House1_Windows_Square;
     public GameObject House1_Windows_Round;
     public GameObject House1_Windows_Wood_Square;
     public GameObject House1_Windows_Wood_Round;

     public GameObject House1_Windows_Square_ExteriourOnly;
     public GameObject House1_Windows_Round_ExteriourOnly;
     public GameObject House1_Windows_Wood_Square_ExteriourOnly;
     public GameObject House1_Windows_Wood_Round_ExteriourOnly;*/

    [Space(30)]
    [Header("House 2")]
    public GameObject House2_Exteriour;
    //public GameObject House2_Exteriour_RoundWindows;
    public GameObject House2_Exteriour_Only;
    public GameObject House2_Interour;
    //public GameObject House2_Interour_RoundWindows;

    [Space(30)]
    [Header("House 3")]
    public GameObject House3_Exteriour;
    //public GameObject House3_Exteriour_RoundWindows;
    public GameObject House3_Exteriour_Only;
    public GameObject House3_Interour;
    //public GameObject House3_Interour_RoundWindows;

    //[Space(30)]
    //[Header("House 4")]
    //public GameObject House4_Exteriour_Only;
    //public GameObject House4_Interour;
    //public GameObject House4_Exteriour;

    [Space(30)]
    [Header("L-Shape House Roofs")]
    public GameObject House1_Roof_Wood;
    public GameObject House1_Roof_Red;
    public GameObject House1_Roof_Blue;
    public GameObject House1_Roof_Grey;
    public GameObject House1_Roof_Planks_Wood;
    public GameObject House1_Roof_Planks_Red;
    public GameObject House1_Roof_Planks_Blue;
    public GameObject House1_Roof_Planks_Grey;

    [Header("Top Window Roofs")]
    public GameObject Small_Roof_Top_Wood;
    public GameObject Small_Roof_Top_Red;
    public GameObject Small_Roof_Top_Blue;
    public GameObject Small_Roof_Top_Grey;

    [Header("Top Window")]
    public GameObject TopWindow;

    [Header("Doors")]
    public GameObject Door;

    [Header("Chimney")]
    public GameObject Chimney;

    [Header("Pergolas")]
    public GameObject PergolaLarge;
    public GameObject PergolaSmall;
    public GameObject PergolaMini;

    [Header("Ivy")]
    public GameObject IvyBottom;
    public GameObject Ivy;

    [Header("Facade")]
    public GameObject House1_Cornerstones;
    public GameObject House1_WoodUnderRoof;

}