namespace GameEngine
{
    public static class GameManager
    {
        public static int delta = 25;
        public static World world = new World(10, 10, 32, 32);
        public static Scene CurrScene { get; private set; } = world.scenes[0, 0];
        static int curSceneX;
        public static int CurSceneX
        {
            get
            {
                return curSceneX;
            }
            set
            {
                curSceneX = value;
                CurrScene = world.scenes[curSceneY, curSceneX];
            }
        }
        static int curSceneY;
        public static int CurSceneY
        {
            get
            {
                return curSceneY;
            }
            set
            {
                curSceneY = value;
                CurrScene = world.scenes[curSceneY, curSceneX];
            }
        }

        public static void ChangeSceneByCoords(int sceneX, int sceneY)
        {
            GameObject? player = CurrScene.FindGameObjectByTag("player");//To move the player to the new screen
            int prevSceneX = CurSceneX;
            int prevSceneY = CurSceneY;
            
            if (sceneX >= 0 && sceneY >= 0 && sceneX < world.scenes.GetLength(1) && sceneY < world.scenes.GetLength(0))
            {
                CurSceneX = sceneX;
                CurSceneY = sceneY;
                if (player != null)
                {
                    CurrScene.InstantiateObject(player); //To move the player to the new screen if the scene has changed
                    world.scenes[prevSceneY,prevSceneX].DestroyObject(player); //Delete the player from the previous scene
                }
            }
            
        }
        public static bool ChangeSceneByDirection(int dir)
        {
            ///<summary>1 = up, 2 = down, 3 = left, 4 = right</summary>
            ///<returns> true if could change screen, false if not </returns>
            bool couldChangeScene = true;
            GameObject? player = CurrScene.FindGameObjectByTag("player");//To move the player to the new screen
            int prevSceneX = CurSceneX;
            int prevSceneY = CurSceneY;

            switch (dir)
            {
                case 1: if (CurSceneY > 0) CurSceneY--; else couldChangeScene = false; break;
                case 2: if (CurSceneY < world.scenes.GetLength(0)) CurSceneY++; else couldChangeScene = false; break;
                case 3: if(CurSceneX > 0) CurSceneX--; else couldChangeScene = false; break;
                case 4: if(CurSceneX < world.scenes.GetLength(1)) CurSceneX++; else couldChangeScene = false; break;
            }
            if (couldChangeScene && player != null)
            {
                CurrScene.InstantiateObject(player); //To move the player to the new screen if scene has changed
                world.scenes[prevSceneY, prevSceneX].DestroyObject(player); //Delete the player from the previous scene
            }
            return couldChangeScene;
        }
    }

    public class GameEngine
    {
        public GameEngine(int dt = 100, int worldWidth = 10, int worldHeight = 10,int screenWidth = 32, int screenHeight = 32)
        {
            GameManager.delta = dt;
            GameManager.world = new World(worldWidth, worldHeight, screenWidth, screenHeight);
        }

        public void RunGameEngine(int startSceneX, int startSceneY)
        {
            GameManager.ChangeSceneByCoords(startSceneX, startSceneY); 
            bool gameRunning = true;

            int frameCount = 0;
            while (gameRunning)
            {
                Input.UpdateInput();
                GameManager.CurrScene.UpdateObjects();
                Renderer.RenderScene();
                Thread.Sleep(GameManager.delta);
                Console.Title = "frames: " + frameCount + " scenePos: X" + GameManager.CurSceneX + " Y" + GameManager.CurSceneY;
                frameCount++;
            }
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
        static char? input;
        public static void UpdateInput()
        {
            if (Console.KeyAvailable)
            {
                input = Console.ReadKey(true).KeyChar;
                while (Console.KeyAvailable) Console.ReadKey(true);
            }
            else
            {
                input = null;
            }
        }
        public static char? GetInput()
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
                    screen[i + posY, j + posX] = sprite[i, j];
                }
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
            Console.SetCursorPosition(GameManager.CurrScene.sceneWidth + 1, 0);
        }
    }
    public class World
    {
        public Scene[,] scenes;
        public World(int wWidth, int wHeight, int sWidth, int sHeight)
        {
            scenes = new Scene[wWidth, wHeight];
            //Fill the world
            for(int i = 0; i < scenes.GetLength(0); i++)
            {
                for(int j = 0; j < scenes.GetLength(1); j++)
                {
                    scenes[i,j] = new Scene(sWidth, sHeight);
                }
            }
        }
        public World(Scene[,] scenes)
        {
            this.scenes = scenes;
        }
        public void ChangeScene(Scene scene, int posX, int posY)
        {
            scenes[posY, posX] = scene;
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
        }

        public void DestroyObject(GameObject gameObject)
        {
            gameObjects.Remove(gameObject);
        }

        public GameObject? FindGameObjectByTag(string tag)
        {
            return gameObjects.Find(x => x.tag == tag);
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

        public GameObject(string tag, int x = 0, int y = 0, int width = 1, int height = 1)
        {
            transform = new Transform(this, x, y, width, height);
            components.Add(transform);
            this.tag = tag;
        }
        public GameObject(string tag, int x = 0, int y = 0, int width = 1, int height = 1, params Component[] components)
        {
            transform = new Transform(this);
            this.components.Add(transform);
            foreach (Component comp in components)
            {
                AddComponent(comp);
            }
            this.tag = tag;
        }

        public virtual void Update()
        {
            foreach (Component component in components)
            {
                component.UpdateComponent();
            }
        }

        public void Destroy()
        {
            GameManager.CurrScene.DestroyObject(this);
        }
    }
}
