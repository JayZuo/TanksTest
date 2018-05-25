using ExitGames.UtilityScripts;
using Photon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankSetup : PunBehaviour
{
    public static Color[] Colors = new Color[] { Color.red, Color.blue, Color.yellow, Color.green };

    /// <summary>Called by PUN on all components of network-instantiated GameObjects.</summary>
    /// <param name="info">Details about this instantiation.</param>
    public override void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        //object color;
        //if (photonView.owner.CustomProperties.TryGetValue("TankColor", out color))
        //{
        //    if (color != null)
        //    {
        //        var colorArray = (float[])color;

        //        // Get all of the renderers of the tank.
        //        MeshRenderer[] renderers = gameObject.GetComponentsInChildren<MeshRenderer>();

        //        // Go through all the renderers...
        //        for (int i = 0; i < renderers.Length; i++)
        //        {
        //            // ... set their material color to the color specific to this tank.
        //            renderers[i].material.color = new Color(colorArray[0], colorArray[1], colorArray[2], colorArray[3]);
        //        }
        //    }
        //}

        int index = info.sender.GetRoomIndex();

        Complete.GameManager.AddTank(gameObject, index, Colors[index], info.sender.NickName);
    }
}
