using System;
using TMPro;
using UnityEngine;

public class LPIPConfigurationMenuController : MonoBehaviour
{
    
    [SerializeField] private TMP_Dropdown cameraDropdown;
    [SerializeField] private TMP_Dropdown projectorIdDropdown;

    [SerializeField] private LPIPUtilityController lpipUtilityController;

    private WebCamDevice[] _webCamDevices;
    private Display[] _displayDevices;

    private void Start()
    {
        RefreshDropdownButtons();
        
        cameraDropdown.onValueChanged.AddListener(CameraDropdownItemSelected);
        projectorIdDropdown.onValueChanged.AddListener(ProjectorIdDropdownItemSelected);
    }

    private void Update()
    {
        if (_webCamDevices.Length != WebCamTexture.devices.Length || _displayDevices.Length != Display.displays.Length)
        {
            Debug.Log("refreshing dropdown");
            RefreshDropdownButtons();
        }
    }

    private void RefreshDropdownButtons()
    {
        cameraDropdown.ClearOptions();
        projectorIdDropdown.ClearOptions();
        
        _webCamDevices = WebCamTexture.devices;
        _displayDevices = Display.displays;
        for (int i = 0; i < _webCamDevices.Length; i++)
        {
            LPIPDropdownData lpipDropdownData = new LPIPDropdownData() {name = _webCamDevices[i].name, id = i };
            cameraDropdown.options.Add(new TMP_DropdownExtended() {text = lpipDropdownData.name, dropdownCustpmData = lpipDropdownData});
        }

        for (int i = 0; i < _displayDevices.Length; i++)
        {
            if (i == 0)
            {
                projectorIdDropdown.options.Add(new TMP_Dropdown.OptionData(){text = $"Primary display"});
            }
            else
            {
                projectorIdDropdown.options.Add(new TMP_Dropdown.OptionData(){text = $"External display {i}"});
            }
        }

        cameraDropdown.value = 0;
        projectorIdDropdown.value = 0;
        
        cameraDropdown.RefreshShownValue();
        projectorIdDropdown.RefreshShownValue();
    }

    private void CameraDropdownItemSelected(int id)
    {
        var name = cameraDropdown.options[id].text;
        Debug.Log($"CameraDropdown Selected item: {name}");
    }
    
    private void ProjectorIdDropdownItemSelected(int id)
    {
        var name = projectorIdDropdown.options[id].text;
        Debug.Log($"projectorIdDropdown Selected item: {name}");
    }

    public void OnSaveConfigurationClick()
    {
        lpipUtilityController.ConfigurationSetupLeave();
    }

    public WebCamDevice GetWebCamDeviceFromDropdown()
    {
        var data = ((TMP_DropdownExtended) cameraDropdown.options[cameraDropdown.value]).dropdownCustpmData;
        return WebCamTexture.devices[data.id];
    }
    
    public int GetProjectorIdFromDropdown()
    {
        return projectorIdDropdown.value;
    }

    public void OnRefreshDropdownButtonClick()
    {
        RefreshDropdownButtons();
    }
}

public class TMP_DropdownExtended : TMP_Dropdown.OptionData
{
    public LPIPDropdownData dropdownCustpmData;
}
public struct LPIPDropdownData
{
    public string name;
    public int id;
}
