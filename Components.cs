using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    public abstract class Component
    {
        public GameObject gameObject;
        public Component(GameObject gameObject) { this.gameObject = gameObject; }
        public virtual void UpdateComponent()
        {

        }
    }
    public class Transform : Component
    {
        public float X0 {  get; private set; }
        float x;
        public float X
        {
            get { return x; }
            set { if (value >= 0) { X0 = X; x = value; } }
        }
        public float Y0 { get; private set; }
        float y;
        public float Y
        {
            get { return y; }
            set { if (value >= 0) { Y0 = Y; y = value; } }
        }
        public int width;
        public int height;
        public float vx;
        public float vy;

        public Transform(GameObject gameObject, float x = 0, float y = 0, int width = 0, int height = 0, float vx = 0, float vy = 0) : base(gameObject)
        {
            X = x;
            Y = y;
            X0 = X;
            Y0 = Y;
            this.width = width;
            this.height = height;
            this.vx = vx;
            this.vy = vy;
        }

        public override void UpdateComponent()
        {
            X += vx * GameManager.delta/1000f;
            Y += vy * GameManager.delta/1000f;
        }
    }

    public class SquareCollider : Component
    {
        public SquareCollider(GameObject gameObject) : base(gameObject)
        {
        }

        public override void UpdateComponent()
        {
            bool collisionDetected = false; //for testing
            List<GameObject> objectsToCheck = new List<GameObject>();
            objectsToCheck.AddRange(GameManager.CurrScene.gameObjects);
            foreach(GameObject go in objectsToCheck)
            {
                if (CollisionDetector.AABCollision(go, gameObject))
                {
                    gameObject.CallCollisionEvent(go);
                    go.CallCollisionEvent(gameObject);
                    collisionDetected = true;
                }
            }
            if (collisionDetected)
            {
                Console.WriteLine("Collision detected");
            }
            else
            {
                Console.WriteLine("                  ");
            }
        }
    }
    public class SpriteRenderer : Component
    {
        char[,] sprite;
        public SpriteRenderer(GameObject gameObject, char[,] sprite) : base(gameObject) { this.sprite = sprite; }
        public SpriteRenderer(GameObject gameObject, char texture) : base(gameObject)
        {
            sprite = new char[gameObject.transform.height,gameObject.transform.width];
            for(int i = 0; i < sprite.GetLength(0); i++) for(int j = 0; j < sprite.GetLength(1); j++) sprite[i,j] = texture;
        }

        public override void UpdateComponent()
        {
            Renderer.AddObjectToScreen((int)gameObject.transform.X, (int)gameObject.transform.Y, sprite);
        }

    }
    public class PlayerMovement : Component
    {
        public PlayerMovement(GameObject gameObject) : base(gameObject) { }

        public override void UpdateComponent()
        {
            char? input = Input.GetInput();

            switch(input)
            {
                case 'a':
                    gameObject.transform.X--;
                    break;
                case 'd':
                    gameObject.transform.X++;
                    break;
                case 'w':
                    gameObject.transform.Y--;
                    break;
                case 's':
                    gameObject.transform.Y++;
                    break;
                default:
                    break;

            }
        }
    }
    public class CollisionManager : Component
    {
        public CollisionManager(GameObject gameObject) : base(gameObject)
        {
            gameObject.OnCollision += WallCollision;
            gameObject.OnCollision += SceneBorderCollision;
        }

        public void WallCollision(GameObject wall)
        {
            if (wall.tag == "wall")
            {
                float desplazamientoX = 0;
                float desplazamientoY = 0; 
                //No es muy bonito pero ahora no se me ocurre nada mejor
                if (gameObject.transform.X0 - gameObject.transform.X > 0) desplazamientoX = 1;
                else if (gameObject.transform.X0 - gameObject.transform.X < 0) desplazamientoX = -1;
                if (gameObject.transform.Y0 - gameObject.transform.Y > 0) desplazamientoY = 1;
                else if (gameObject.transform.Y0 - gameObject.transform.Y < 0) desplazamientoY = -1;
                do
                {
                    gameObject.transform.X += desplazamientoX;
                    gameObject.transform.Y += desplazamientoY;
                } while (CollisionDetector.AABCollision(gameObject, wall));
            }
        }
        public void SceneBorderCollision(GameObject sceneBorder)
        {
            switch(sceneBorder.tag)
            {
                case "sceneBorderNorth": if(GameManager.ChangeSceneByDirection(1)) gameObject.transform.Y = GameManager.CurrScene.sceneHeight - 2; break;
                case "sceneBorderSouth": if(GameManager.ChangeSceneByDirection(2)) gameObject.transform.Y = 1;  break;
                case "sceneBorderWest": if(GameManager.ChangeSceneByDirection(3)) gameObject.transform.X = GameManager.CurrScene.sceneWidth - 2; break;
                case "sceneBorderEast": if(GameManager.ChangeSceneByDirection(4)) gameObject.transform.X = 1; break;
                default: break;
            }
        }
    }
}
