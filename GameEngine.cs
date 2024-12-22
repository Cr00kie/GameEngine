namespace GameEngine
{
    public static class GameEngine
    {
        public static int delta = 25;
        public static Scene CurrScene { get; private set; }

        public static void SetProperties(int dt = 100, int screenWidth = 32, int screenHeight = 32)
        {
            GameEngine.delta = dt;
            ChangeScene( new Scene(screenWidth, screenHeight));
        }

        public static void RunGameEngine(Scene startingScene)
        {
            ChangeScene(startingScene);
            bool gameRunning = true;

            int frameCount = 0;
            while (gameRunning)
            {
                Input.UpdateInput();
                GameEngine.CurrScene.UpdateObjects();
                Renderer.RenderScene();
                Thread.Sleep(GameEngine.delta);
                Console.Title = "frames: " + frameCount;
                frameCount++;
            }
        }

        public static void ChangeScene(Scene newScene)
        {
            if (CurrScene != null)
            {
                List<GameObject> gameObjectsSavedBetweenScreens = CurrScene.gameObjects.Where(x => x.keepWhenChangingScreens).ToList();
                for (int i = 0; i < gameObjectsSavedBetweenScreens.Count; i++)
                {
                    newScene.InstantiateObject(gameObjectsSavedBetweenScreens[i]);
                    CurrScene.DestroyObject(gameObjectsSavedBetweenScreens[i]);
                }
                CurrScene = newScene;
            }
            else 
            {
                CurrScene = newScene;
            }

            Renderer.screen = new char[CurrScene.sceneHeight, CurrScene.sceneWidth];
        }
    }

    public static class CollisionDetector
    {
        public static bool AABCollision(GameObject gameObject1, GameObject gameObject2)
        {
            int x1L = (int)gameObject1.transform.X;
            int x1R = (int)gameObject1.transform.X + gameObject1.transform.width;
            int x2L = (int)gameObject2.transform.X;
            int x2R = (int)gameObject2.transform.X + gameObject2.transform.width;

            int y1U = (int)gameObject1.transform.Y;
            int y1D = (int)gameObject1.transform.Y + gameObject1.transform.height;
            int y2U = (int)gameObject2.transform.Y;
            int y2D = (int)gameObject2.transform.Y + gameObject2.transform.height;

            return (((x1L < x2R && x1L >= x2L)
               || (x2L < x1R && x2L >= x1L))
               &&
               ((y1U < y2D && y1U >= y2U)
               || (y2U < y1D && y2U >= y1U))
               && !gameObject2.Equals(gameObject1)
              );
        }
    }

    public static class Input
    {
        static ConsoleKey input;
        public static void UpdateInput()
        {
            if (Console.KeyAvailable)
            {
                input = Console.ReadKey(true).Key;
                while (Console.KeyAvailable) Console.ReadKey(true);
            }
            else
            {
                input = ConsoleKey.None;
            }

        }
        public static ConsoleKey GetInput()
        {
            return input;
        }
    }

    public static class Renderer
    {
        public static char[,] screen = new char[32,32];
        public static void AddObjectToScreen(int posX, int posY, char[,] sprite)
        {
            for (int i = 0; i<sprite.GetLength(0) && i+posY < screen.GetLength(0); i++)
            {
                for (int j = 0; j<sprite.GetLength(1) && j + posX < screen.GetLength(1); j++)
                {
                    if(i+posY >= 0 && j + posX >= 0) screen[i + posY, j + posX] = sprite[i, j];
                }
            }
        }
        public static void DrawPoint(int x, int y, char texture)
        {
            if (y >= 0 && y < screen.GetLength(0) && x >= 0 && x < screen.GetLength(1)) screen[y, x] = texture;
        }
        public static void DrawLine(float x1, float y1, float x2, float y2, char texture)
        {
            float ux = x2 - x1, uy = y2 - y1;
            float endLoop = Math.Abs(ux) > Math.Abs(uy) ? Math.Abs(ux) : Math.Abs(uy);
            float xi = 0, yi = 0;
            while(Math.Abs(xi) <= endLoop && Math.Abs(yi) <= endLoop)
            {
                DrawPoint((int)Math.Round(xi+x1),(int)Math.Round(yi+y1),texture);
                xi += ux / endLoop;
                yi += uy / endLoop;
            }
        }
        public static void RenderScene()
        {
            //Write the screen
            Console.CursorVisible = false;
            string buffer = "";
            for (int i = 0; i < screen.GetLength(0); i++)
            {
                for (int j = 0; j < screen.GetLength(1); j++)
                {
                    buffer += screen[i, j];
                }
                buffer += "\n";
            }
            Console.SetCursorPosition(0, 0);
            Console.WriteLine(buffer);
            Console.SetCursorPosition(0, 0);

            //Clear the screen
            for (int i = 0; i < screen.GetLength(0); i++)
            {
                for (int j = 0; j < screen.GetLength(1); j++)
                {
                    screen[i, j] = ' ';
                }
            }
            Console.SetCursorPosition(GameEngine.CurrScene.sceneWidth + 1, 0);
        }
    }
    public static class Debugger
    {
        const int maxMessages = 10;
        static List<String> messages = new List<String>();

        public static void Write(string msg)
        {
            if(messages.Count >= maxMessages) messages.RemoveAt(0);
            messages.Add(msg);
            string debuggerMessages = "Debugger: \n";
            for (int i = messages.Count - 1; i >= 0; i--)
            {
                debuggerMessages += $"{messages.Count-i}| {DateTime.Now} - {messages[i]}                                                                    \n";
            }
            Console.SetCursorPosition(0, GameEngine.CurrScene.sceneHeight + 1);
            Console.WriteLine(debuggerMessages);
        }
    }
    
    public class Scene
    {
        public int sceneWidth, sceneHeight;
        public Scene(int sWidth, int sHeight)
        {
            sceneWidth = sWidth;
            sceneHeight = sHeight;
        }
        public List<GameObject> gameObjects = new List<GameObject>();

        public void InstantiateObject(GameObject gameObject, int posX, int posY)
        {
            gameObjects.Add(gameObject);
            gameObject.transform.X = posX;
            gameObject.transform.Y = posY;
        }
        public void InstantiateObject(GameObject gameObject)
        {
            gameObjects.Add(gameObject);
            gameObject.OnInstantiation(this);
        }

        public void DestroyObject(GameObject gameObject)
        {
            gameObjects.Remove(gameObject);
        }

        public GameObject? FindGameObjectByTag(string tag)
        {
            return gameObjects.Find(x => x.tag == tag);
        }
        public GameObject? FindGameObjectByComponent(Type component)
        {
            return gameObjects.Find(x => x.GetComponent(component) != null);
        }

        public void UpdateObjects()
        {
            List<GameObject> objects = new List<GameObject>();
            objects.AddRange(gameObjects);
            foreach (GameObject gameObject in objects)
            {
               gameObject.Update();
            }
        }
    }

    public class GameObject
    {
        List<Component> components = new List<Component>();

        public Transform transform;

        public event Action<GameObject>? OnCollision;

        public string tag;

        public bool keepWhenChangingScreens = false;

        public void CallCollisionEvent(GameObject obj)
        {
            if (OnCollision != null) OnCollision(obj);
        }

        public void AddComponent(Component componentToAdd)
        {
            if(!components.Contains(componentToAdd)) components.Add(componentToAdd);
        }
        public void RemoveComponent(Component componentToRemove)
        {
            components.Remove(componentToRemove);
        }
        public Component? GetComponent<T>()
        {
            return components.Find(x => x.GetType() == typeof(T));
        }
        public Component? GetComponent(Type type)
        {
            return components.Find(x => x.GetType() == type);
        }

        public GameObject(string tag, int x = 0, int y = 0, int width = 1, int height = 1, bool keepWhenChangingScreens = false)
        {
            transform = new Transform(this, x, y, width, height);
            components.Add(transform);
            this.tag = tag;
            this.keepWhenChangingScreens = keepWhenChangingScreens;
        }
        public GameObject(string tag, int x = 0, int y = 0, int width = 1, int height = 1, bool keepWhenChangingScreens = false ,params Component[] components)
        {
            transform = new Transform(this);
            this.components.Add(transform);
            this.keepWhenChangingScreens = keepWhenChangingScreens;
            foreach (Component comp in components)
            {
                AddComponent(comp);
            }
            this.tag = tag;
        }

        public virtual void Update()
        {
            List<Component> objects = new List<Component>();
            objects.AddRange(components);
            foreach (Component component in objects)
            {
                component.UpdateComponent();
            }
        }

        public virtual void OnInstantiation(Scene sceneInstatiatedIn)
        {
            foreach (Component component in components)
            {
                component.OnInstantiation(sceneInstatiatedIn);
            }
        }

        public void Destroy()
        {
            GameEngine.CurrScene.DestroyObject(this);
        }
    }
}
