using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using HoloToolkit.Unity;
using TMPro;
using HoloToolkit.Unity.InputModule;
using UnityEngine.Video;

/// <summary>
/// 
/// This script is home to all of the voice commands that the user has access too, excluding the main menu commands
/// 
/// Author: Intern, Arlan Ohrt, Summer 2018
/// 
/// </summary>
public class CommandLibrary : MonoBehaviour
{
    public GameObject explosive;
    public GameObject mainMenu;
    public GameObject commandList;
    public GameObject showCommandsText;
    public GameObject hideCommandsText;
    public GameObject surroundings;
    public GameObject videoRectangle;
    public Slider destructionMeter;
    public TextMeshProUGUI destructionText;
    public GameObject MRTKobject;

    //The y position for the camera to jump to, it is 6 ft up in meters
    float groundViewHeight = 1.2f;

    GameObject newExplosive;

    MenuScript mainMenuScript;

    VideoPlayer video;

    private void Start()
    {
         mainMenuScript = mainMenu.GetComponent<MenuScript>();
        videoRectangle.SetActive(false);
    }

    private bool placed = false;

    /// <summary>
    /// OnJumpTo will "teleport" the user to where they are looking.
    /// </summary>
    public void OnJumpTo()
    {
        // Do a raycast into the world based on the user's head position and orientation.
        var headPosition = Camera.main.transform.position;
        var gazeDirection = Camera.main.transform.forward;

        RaycastHit hitInfo;
            if (Physics.Raycast(headPosition, gazeDirection, out hitInfo))
            {
                //Get the distance between hitInfo.point and the position that the world is at
                Vector3 distance = Camera.main.transform.position - hitInfo.point;

                //Move the world this distance so that this position will be under the user
                surroundings.transform.position = surroundings.transform.position + distance + (-Vector3.up * groundViewHeight);
            }
    }

    /// <summary>
    /// OnGoTo will put the user's view at the position of the GameObject with the given tag.
    /// Recommended: The tag of the object be unique - if more than one object have the tag, 
    /// the first object in the scene with the tag will be where the user goes.
    /// </summary>
    /// <param name="destination">The tag of the object that the user wants to go to</param>
    public void OnGoTo(string destination)
    {
        if (GameObject.FindGameObjectWithTag(destination))
        {
            GameObject dest = GameObject.FindGameObjectWithTag(destination);

            //Get the distance between the destination position and the position that the world is at
            Vector3 distance = Camera.main.transform.position - dest.transform.position;

            //Move the world this distance so that this position will be under the user
            surroundings.transform.position = surroundings.transform.position + distance + (-Vector3.up * groundViewHeight);

            // Set the rotation of the camera for proper viewing
            Camera.main.transform.rotation = dest.transform.rotation;

            if(destination == "Menu")
            {
                mainMenuScript.activeOverlay.SetActive(false);
                mainMenu.SetActive(true);
                mainMenu.transform.Rotate(Vector3.up, 90.0f);
            }
        }

    }

    /// <summary>
    /// ResetScene will take the current ousition of the user and reset the scene completely, at the users position in real space
    /// </summary>
    public void ResetScene()
    {
        Destroy(MRTKobject);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        GameObject mainMenu = GameObject.Find("MainMenu");
        GameObject activeOverlay = GameObject.Find("Active Overlay");
        activeOverlay.SetActive(false);
        mainMenu.SetActive(true);
        mainMenu.transform.Rotate(Vector3.up, 90.0f);
    }

    /// <summary>
    /// HideCommands will swap the permanent overlay title's text to "Show Commands" and hide the command list
    /// </summary>
    public void HideCommands()
    {
        // Deactivate command list and activate/deactivate the propper text
        commandList.SetActive(false);
        showCommandsText.SetActive(true);
        hideCommandsText.SetActive(false);
    }

    /// <summary>
    /// ShowCommands will swap the permanent overlay title's text to "Hide Commands" and show the command list
    /// </summary>
    public void ShowCommands()
    {
        // Deactivate command list and activate/deactivate the propper text
        commandList.SetActive(true);
        showCommandsText.SetActive(false);
        hideCommandsText.SetActive(true);
    }

    /// <summary>
    /// PlaceExplosive gets the users gaze and when called, will place the explosive on the object it is looking at
    /// </summary>
    public void PlaceExplosive()
    {
        // Do a raycast into the world based on the user's head position and orientation.
        var headPosition = Camera.main.transform.position;
        var gazeDirection = Camera.main.transform.forward;
        
        // Perform raycast
        RaycastHit hitInfo;
        
        if(Physics.Raycast(headPosition, gazeDirection, out hitInfo) && !placed)
        {
            newExplosive = Instantiate(explosive, hitInfo.point, Quaternion.FromToRotation(transform.up, hitInfo.normal));
            placed = true;
            
        }
    }

    /// <summary>
    /// DetonateExplosive sends information to RealisticExplosion.cs which actually performs the explosion
    /// </summary>
    public void DetonateExplosive()
    {

        newExplosive.GetComponent<RealisticExplosion>().Explode(mainMenuScript.explosiveType, mainMenuScript.explosiveLocation,
            mainMenuScript.minimumRelevantPressure, mainMenuScript.explosiveWeight);

        placed = false;

        destructionMeter.value = (newExplosive.GetComponent<RealisticExplosion>().counter) / 10 ;
        destructionText.text = destructionMeter.value + " %";
    }

    /// <summary>
    /// ShowFloorplan will bring up the floorplan of the Ladeburg
    /// </summary>
    public void ShowFloorplan()
    {
        mainMenuScript.floorplan.SetActive(true);
        mainMenuScript.activeOverlay.SetActive(false);
    }

    /// <summary>
    /// HideFloorplan will hide the floorplan of the Ladeburg
    /// </summary>
    public void HideFloorplan()
    {
        mainMenuScript.floorplan.SetActive(false);
        mainMenuScript.activeOverlay.SetActive(true);
    }

    /// <summary>
    /// PlayVideo will bring up the video and its commands, it should hide the other things in active overlay
    /// </summary>
    public void PlayVideo()
    {
        video = gameObject.GetComponent<VideoPlayer>();
        videoRectangle.SetActive(true);
        video.Play();
        StartCoroutine(Player());
    }
    /// <summary>
    /// PLayer() is a coroutine for PlayVideo
    /// </summary>
    /// <returns></returns>
    IEnumerator Player()
    {
        yield return new WaitForSecondsRealtime(18);
        video.Stop();
        videoRectangle.SetActive(false);
    }

}
