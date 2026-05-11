using UnityEngine;

public static class SnakeGameBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        EnsureCamera();

        GameObject root = GameObject.Find("Snake Game Runtime");
        if (root == null)
            root = new GameObject("Snake Game Runtime");

        EnsureComponent<GridManager>(root);
        EnsureComponent<SnakeController>(root);
        EnsureComponent<FoodSpawner>(root);
        EnsureComponent<ScoreManager>(root);
        EnsureComponent<GameManager>(root);
        EnsureComponent<InputManager>(root);
        EnsureComponent<UIManager>(root);
    }

    private static void EnsureCamera()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
        }

        camera.orthographic = true;
        camera.orthographicSize = 12.5f;
        camera.transform.position = new Vector3(0f, 0f, -10f);
        camera.backgroundColor = new Color(0.09f, 0.13f, 0.16f);
        camera.clearFlags = CameraClearFlags.SolidColor;
    }

    private static T EnsureComponent<T>(GameObject root) where T : Component
    {
        T existing = Object.FindObjectOfType<T>();
        if (existing != null)
            return existing;

        return root.AddComponent<T>();
    }
}
