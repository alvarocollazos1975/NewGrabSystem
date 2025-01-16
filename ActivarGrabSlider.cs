using BIMOS;
using UnityEngine;

public class ActivarGrabSlider : MonoBehaviour
{
    
    public GameObject Grab1;
    public GameObject Grab2;
  
    public void ActivarGrab()
    {

        Grab1.SetActive(true);
        Grab2.SetActive(true);


    }
    public void DesActivarGrab()
    {

        Grab1.SetActive(false);
        Grab2.SetActive(false);


    }
}
