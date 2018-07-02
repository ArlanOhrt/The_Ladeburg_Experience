using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using HoloToolkit.Unity;
using TMPro;

// Explosive options
public enum ExplosiveType
    {
        TNT, PETN
    }

public enum ExplosiveLocation
    {
        ExteriorWall, InteriorWall
    }

/// <summary>
/// 
/// This script handles all aspects of the main menu including all main menu selctions the user must make, the "Menu Help" command,
/// and storing the selected values for RealisticExplosion.cs to grab and use
/// 
/// Author: Intern, Arlan Ohrt, Summer 2018
/// 
/// </summary>
public class MenuScript : MonoBehaviour {

    public TMP_Dropdown explosiveDropdown;
    public TMP_Dropdown explosiveTargetDropdown;

    public Slider weightSlider;
    public Slider pressureSlider;

    public GameObject activeOverlay;
    public GameObject floorplan;
    public GameObject showCommandsText;
    public GameObject hideCommandsText;

    public TextMeshProUGUI textWeight;
    public TextMeshProUGUI textPressure;
    public TextMeshProUGUI commandsText;

    // Change speakText to alter what the "Menu Help" command outputs
    private string speakText = " Use the explosion type dropdown menu to select the type of explosive you want to use. Use the Target Option" +
        " dropdown menu to select if you would like to do the test on the interior or exterior target." +
        " Use the explosive weight slider to pick the weight of the explosive in pounds, and note if you" +
        " did not pick TNT, do not convert the weight." +
        " Use the minimum relevant pressure slider to pick the minimum pressure you care for the test to search to." +
        " The smaller this number is the more accurate the test is, however larger numbers run smoother." +
        " Once you select all of your test preferences, click on begin to go to the next step.";

    // Change beginText to to alter what will be said after the begin button is pushed
    private string beginText = "Now that you have made your selections, you can place the charge where you" +
        " would like too, and detonate it. Look around for the target wall. You have been teleported near the target you selected. " +
        " You have a list of voice commands to the left that will help you navigate and interact with your surroundings." +
        " The target walls appear as brick, and they are the only destructible objects. After you detonate the " +
        " charge, you will see how your menu selections and placement effected the target wall from the" +
        " target destruction meter at the bottom of your view.";

    // Change exteriorText to alter the command list that will come up when the user selects exterior target
    private string exteriorText = "'Jump To' - Gets where your cursor is and takes you there\n\n" +
        "'Teleport Inside' - Brings you inside to place the explosive\n\n" +
        "'Teleport Outside' - Brings you outside to place the explosive\n\n" +
        "'Teleport Menu' - Brings you back to the main menu\n\n" + 
        "'Place Explosive' - Places the explosive where your cursor is\n\n" +
        "'Detonate' - Detonates explosive\n\n" +
        "'Play Video' - Brings up a video of the real Ladeburg test, it will disappear after playing\n\n" +
        "'Show Floorplan' - Brings up the Ladeburg's Floorplan\n\n" +
        "'Reset Scene' - Brings you back to the main menu to start again";

    // Change interiorText to alter the command list that will come up when the user selects interior target
    private string interiorText = "'Jump To' - Gets where your cursor is and takes you there\n\n" +
        "'Teleport Room' - Brings you into the target wall's room\n\n" +
        "'Teleport Hallway' - Brings you to a hallway to view the target wall's backside\n\n" +
        "'Teleport Menu' - Brings you back to the main menu\n\n" +
        "'Place Explosive' - Places the explosive where your cursor is\n\n" +
        "'Detonate' - Detonates explosive\n\n" +
        "'Play Video' - Brings up a video of the real Ladeburg test, it will disappear after playing\n\n" +
        "'Show Floorplan' - Brings up the Ladeburg's Floorplan\n\n" +
        "'Reset Scene' - Brings you back to the main menu to start again";

    private TextToSpeech textToSpeech;

    public ExplosiveType explosiveType = ExplosiveType.PETN;
    public ExplosiveLocation explosiveLocation = ExplosiveLocation.ExteriorWall;
    float minimumRelevantPressureMin = 5.0f;
    float minimumRelevantPressureMax = 10.0f;
    public float minimumRelevantPressure = 7.5f;
    float explosiveWeightMin = 0.001f;
    float explosiveWeightMax = 25.0f;
    public float explosiveWeight = 4.41f;

    // Use this for initialization
    void Start()
    {
        textToSpeech = GetComponent<TextToSpeech>();

        // Make sure activeOverlay and floorplan is off;
        activeOverlay.SetActive(false);
        floorplan.SetActive(false);

        PopulateList();

        // Set slider values based on public variables
        weightSlider.minValue = explosiveWeightMin;
        weightSlider.maxValue = explosiveWeightMax;
        weightSlider.value = explosiveWeight;

        pressureSlider.minValue = minimumRelevantPressureMin;
        pressureSlider.maxValue = minimumRelevantPressureMax;
        pressureSlider.value = minimumRelevantPressure;

        // Set Dropdown options based on public variables
        if (explosiveType == ExplosiveType.TNT)
            explosiveDropdown.value = 0;
        else if (explosiveType == ExplosiveType.PETN)
            explosiveDropdown.value = 1;

        if (explosiveLocation == ExplosiveLocation.ExteriorWall)
            explosiveTargetDropdown.value = 0;
        else if (explosiveLocation == ExplosiveLocation.InteriorWall)
            explosiveTargetDropdown.value = 1;
    }

    // Fill the dropdown menus with the global enum options
    void PopulateList()
    {
        string[] enumExplosive = Enum.GetNames(typeof(ExplosiveType));
        List<string> names = new List<string>(enumExplosive);
        explosiveDropdown.AddOptions(names);

        string[] enumExplosiveLocation = Enum.GetNames(typeof(ExplosiveLocation));
        List<string> locationNames = new List<string>(enumExplosiveLocation);
        explosiveTargetDropdown.AddOptions(locationNames);
    }

    // Sets explosiveWeight to weight slider value
    public void SliderControlWeight(float value)
    {
        explosiveWeight = value;
    }

    // Sets the text next to weight slider
    public void ChangeTextWeight(float value)
    {
        float rounded = Mathf.Round(value * 100.0f) / 100.0f;
        textWeight.text = rounded + " lbs";
    }

    // Sets minimumRelevantPressure to pressure slider value
    public void SliderControlPressure(float value)
    {
        minimumRelevantPressure = value;
    }

    // Sets the text next to pressure slider
    public void ChangeTextPressure(float value)
    {
        float rounded = Mathf.Round(value * 100.0f) / 100.0f;
        textPressure.text = rounded + " psi";
    }

    // What happens when ExplosiveType dropdown changes
    public void ExplosiveTypeDropdown()
    {
        if (explosiveDropdown.value == 0)
            explosiveType = ExplosiveType.TNT;
        else if (explosiveDropdown.value == 1)
            explosiveType = ExplosiveType.PETN;
    }

    // What happens when explosivePlacementDropdown changes
    public void LocationDropdown()
    {
        if (explosiveTargetDropdown.value == 0)
        {
            explosiveLocation = ExplosiveLocation.ExteriorWall;
            Debug.Log(explosiveLocation);
        }

        else if (explosiveTargetDropdown.value == 1)
        {
            explosiveLocation = ExplosiveLocation.InteriorWall;
            Debug.Log(explosiveLocation);
        }
    }

    // What happens when the user says "Menu Help"
    public void StartHelp()
    {
        // Create Message
        var startMsg = string.Format(speakText, textToSpeech.Voice.ToString());

        // Speak Message
        textToSpeech.StartSpeaking(startMsg);
    }

    // What happens once begin button is clicked
    public void BeginButton()
    {
        // Deactivate main menu and bring up the overlay
        gameObject.SetActive(false);
        activeOverlay.SetActive(true);
        showCommandsText.SetActive(false);
        hideCommandsText.SetActive(true);

        // Start the instructions
        var beginMsg = string.Format(beginText, textToSpeech.Voice.ToString());
        textToSpeech.StartSpeaking(beginMsg);

        // Populate the command list
        if (explosiveLocation == ExplosiveLocation.ExteriorWall)
            commandsText.text = exteriorText;
        else if (explosiveLocation == ExplosiveLocation.InteriorWall)
            commandsText.text = interiorText;

        // Teleport user
        string destination = "";

        if (explosiveLocation == ExplosiveLocation.ExteriorWall)
            destination = "Outside";
        else if (explosiveLocation == ExplosiveLocation.InteriorWall)
            destination = "Room";
        activeOverlay.GetComponent<CommandLibrary>().OnGoTo(destination);
    }
}
