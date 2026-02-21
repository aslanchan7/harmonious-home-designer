using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class IconThumbnailEditor : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Tools/Icon Editor")]
    public static void ShowExample()
    {
        IconThumbnailEditor wnd = GetWindow<IconThumbnailEditor>();
        wnd.titleContent = new GUIContent("IconThumbnailEditor");
    }

    private ListView listView;
    [SerializeField] private InventoryScriptableObject inventory;
    private List<string> furnitureListNames = new();
    [SerializeField] private InventoryItem selectedAsset;
    [SerializeField] private string selectedAssetName = "None Selected";
    [SerializeField] private Texture2D previewTexture;
    private Vector3Field cameraPosField;
    private Vector3Field cameraRotationField;
    private Slider objectRotationSlider;
    private Button saveButton;

    // Preview
    private int size = 512;
    private Scene previewScene;
    private GameObject cameraObject;
    private Camera sceneCamera;
    private GameObject instance;

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);

        // Set List View & assign inventory items to it
        listView = rootVisualElement.Q<ListView>("List");
        foreach (var furniture in inventory.inventoryList)
        {
            furnitureListNames.Add(furniture.Name);
        }
        listView.itemsSource = furnitureListNames;

        listView.selectionChanged += OnSelectItem;

        rootVisualElement.Q<VisualElement>("Content").dataSource = this;

        cameraPosField = rootVisualElement.Q<Vector3Field>("CameraPos");
        cameraRotationField = rootVisualElement.Q<Vector3Field>("CameraRotation");

        cameraPosField.RegisterValueChangedCallback(OnCameraPositionChange);
        cameraRotationField.RegisterValueChangedCallback(OnCameraRotationChange);

        objectRotationSlider = rootVisualElement.Q<Slider>("ObjectRotation");
        objectRotationSlider.RegisterValueChangedCallback(OnObjectRotationChange);

        saveButton = rootVisualElement.Q<Button>("SaveButton");
        saveButton.clicked += Export;
    }

    void Export()
    {
        sceneCamera.depthTextureMode = DepthTextureMode.Depth;
        sceneCamera.backgroundColor = new (0, 0, 0, 0);
        UpdateCamera();

        SaveTextureAsPNG(previewTexture, selectedAsset.Name);

        sceneCamera.backgroundColor = Color.black;
        UpdateCamera();
    }

    void SaveTextureAsPNG(Texture2D texture, string filename)
    {
        if(texture == null)
        {
            Debug.LogError("No texture was provided");
            return;
        }

        string path = EditorUtility.SaveFilePanel("Save Texture As PNG", "", $"{filename}_Icon.png", "png");
        if(string.IsNullOrEmpty(path))
        {
            Debug.Log("Save operation cancelled");
            return;
        }

        byte[] pngData = texture.EncodeToPNG();
        if(pngData != null)
        {
            File.WriteAllBytes(path, pngData);
            Debug.Log("Texture saved to: " + path);
        } else
        {
            Debug.LogError("Failed to encode texture to PNG.");
        }
    }

    void OnObjectRotationChange(ChangeEvent<float> evt)
    {
        instance.transform.eulerAngles = new Vector3(0, evt.newValue, 0);
        UpdateCamera();
    }

    void OnCameraPositionChange(ChangeEvent<Vector3> evt)
    {
        cameraObject.transform.position = evt.newValue;
        UpdateCamera();
    }

    void OnCameraRotationChange(ChangeEvent<Vector3> evt)
    {
        cameraObject.transform.eulerAngles = evt.newValue;
        UpdateCamera();
    }

    private void OnSelectItem(object item)
    {
        selectedAsset = inventory.inventoryList[listView.selectedIndex];
        selectedAssetName = selectedAsset.Name;

        if(!previewScene.IsValid())
        {
            previewScene = EditorSceneManager.NewPreviewScene();
        }

        if(cameraObject == null)
        {
            cameraObject = new GameObject("Camera");
            cameraObject.transform.position = new Vector3(0, 0, -10);
            cameraObject.transform.eulerAngles = new Vector3(0, 0, 0);

            sceneCamera = cameraObject.AddComponent<Camera>();
            sceneCamera.aspect = 1f;
            sceneCamera.backgroundColor = Color.black;
            sceneCamera.clearFlags = CameraClearFlags.SolidColor;
            sceneCamera.targetTexture = new RenderTexture(size, size, 32, RenderTextureFormat.ARGBFloat);

            SceneManager.MoveGameObjectToScene(cameraObject, previewScene);

            sceneCamera.scene = previewScene;

            cameraPosField.value = cameraObject.transform.position;
            cameraRotationField.value = cameraObject.transform.eulerAngles;
        }

        if(instance != null)
        {
            DestroyImmediate(instance);
        }

        instance = (GameObject)PrefabUtility.InstantiatePrefab(selectedAsset.Prefab, previewScene);
        instance.transform.rotation = Quaternion.identity;
        instance.transform.position = Vector3.zero;

        UpdateCamera();
    }

    private void UpdateCamera()
    {
        sceneCamera.Render();

        if(previewTexture == null)
        {
            previewTexture = new(size, size, TextureFormat.RGBAFloat, false, true);
        }

        RenderTexture.active = sceneCamera.targetTexture;

        previewTexture.ReadPixels(new Rect(0, 0, size, size), 0, 0);
        previewTexture.Apply();

        RenderTexture.active = null;
    }

    void OnDisable()
    { 
        EditorSceneManager.UnloadSceneAsync(previewScene);
        DestroyImmediate(sceneCamera);
    }
}
