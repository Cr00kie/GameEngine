using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
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
        public virtual void OnInstantiation(Scene sceneInstatiatedIn) { }
    }
    public class Transform : Component
    {
        public float X0 {  get; private set; }
        float x;
        public float X
        {
            get { return x; }
            set { X0 = X; x = value; }
        }
        public float Y0 { get; private set; }
        float y;
        public float Y
        {
            get { return y; }
            set { Y0 = Y; y = value; }
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
            X += vx * GameEngine.delta/1000f;
            Y += vy * GameEngine.delta/1000f;
        }

        public bool MoveTowards(float x, float y, float speed)
        {
            ///<summary>Moves object towards x and y gradually</summary>
            if ((int)x == (int)X && (int)y == (int)Y)
            {
                X = x; Y = y;
                return true;
            }
            X += (x - X) * speed * GameEngine.delta/1000;
            Y += (y - Y) * speed * GameEngine.delta/1000;

            return false; // returns true when point reached
        }
    }

    public class SquareCollider : Component
    {
        public bool isTrigger;
        public SquareCollider(GameObject gameObject, bool isTrigger) : base(gameObject)
        {
            this.isTrigger = isTrigger;
        }

        public override void UpdateComponent()
        {
            List<GameObject> objectsToCheck = new List<GameObject>();
            objectsToCheck.AddRange(GameEngine.CurrScene.gameObjects.Where(x=> x.GetComponent<SquareCollider>() != null)); //Only check objects that have colliders (It could do it with objects without collider but i guess if you don't give them a collider is because you dont want it to collide)
            foreach(GameObject go in objectsToCheck)
            {
                if (CollisionDetector.AABCollision(go, gameObject))
                {
                    gameObject.CallCollisionEvent(go);
                    go.CallCollisionEvent(gameObject);
                    Debugger.Write($"Collision Detected between {gameObject.tag}({gameObject.transform.X}, {gameObject.transform.Y}) and {go.tag}({go.transform.X}, {go.transform.Y})");
                    SquareCollider goCollider = (SquareCollider)go.GetComponent<SquareCollider>();
                    if(!goCollider.isTrigger) SeparateObjects(go);
                }
            }
        }

        public void SeparateObjects(GameObject objCollided)
        {
            float desplazamientoX = 0;
            float desplazamientoY = 0;
            //No es muy bonito pero ahora no se me ocurre nada mejor
            if (gameObject.transform.X0 - gameObject.transform.X > 0) desplazamientoX = 1;
            else if (gameObject.transform.X0 - gameObject.transform.X < 0) desplazamientoX = -1;
            if (gameObject.transform.Y0 - gameObject.transform.Y > 0) desplazamientoY = 1;
            else if (gameObject.transform.Y0 - gameObject.transform.Y < 0) desplazamientoY = -1;
            if (desplazamientoX != 0 && desplazamientoY != 0) // if we don't check this it will be stuck in a loop forever
            {
                do
                {
                    gameObject.transform.X += desplazamientoX;
                    gameObject.transform.Y += desplazamientoY;

                } while (CollisionDetector.AABCollision(gameObject, objCollided));
            }

        }
    }
    public class SpriteRenderer : Component
    {
        public char[,] sprite;
        public int xOffSet;
        public int yOffSet;
        public SpriteRenderer(GameObject gameObject, char[,] sprite, int xOffSet = 0, int yOffSet = 0) : base(gameObject)
        {
            this.sprite = sprite;
            this.xOffSet = xOffSet;
            this.yOffSet = yOffSet;
        }
        public SpriteRenderer(GameObject gameObject, char texture) : base(gameObject)
        {
            sprite = new char[gameObject.transform.height,gameObject.transform.width];
            for(int i = 0; i < sprite.GetLength(0); i++) for(int j = 0; j < sprite.GetLength(1); j++) sprite[i,j] = texture;
        }
        public SpriteRenderer(GameObject gameObject, string text, int xOffSet = 0, int yOffSet = 0) : base(gameObject)
        {
            this.xOffSet = xOffSet;
            this.yOffSet = yOffSet;
            sprite = new char[1, text.Length];
            for(int i = 0; i < text.Length; i++) sprite[0,i] = text[i];
        }

        public override void UpdateComponent()
        {
            Renderer.AddObjectToScreen((int)gameObject.transform.X + xOffSet, (int)gameObject.transform.Y + yOffSet, sprite);
        }

    }
    public class ResizableBoxRenderer : SpriteRenderer
    {
        char upLeft, upRight, downLeft, downRight, ver, hor;
        public char fill;
        public bool doubleLine;
        public ResizableBoxRenderer(GameObject gameObject, bool doubleLine = false, char fill = ' ') : base(gameObject, new char[gameObject.transform.height,gameObject.transform.width])
        {
            this.doubleLine = doubleLine;
            this.fill = fill;
            sprite = MakeBox();
        }
        public char[,] MakeBox()
        {
            char[,] ret = new char[gameObject.transform.height, gameObject.transform.width];
            if (doubleLine)
            {
                upLeft = '╔';
                upRight = '╗';
                downLeft = '╚';
                downRight = '╝';
                ver = '║';
                hor = '═';
            }
            else
            {
                upLeft = '┌';
                upRight = '┐';
                downLeft = '└';
                downRight = '┘';
                ver = '│';
                hor = '─';
            }
            for (int i = 0; i < gameObject.transform.height; i++)
            {
                for (int j = 0; j < gameObject.transform.width; j++)
                {
                    if (i == 0 || i == gameObject.transform.height - 1) ret[i, j] = hor;
                    else if (j == 0 || j == gameObject.transform.width - 1) ret[i, j] = ver;
                    else ret[i, j] = fill;
                }
            }
            ret[0, 0] = upLeft;
            ret[0, gameObject.transform.width - 1] = upRight;
            ret[gameObject.transform.height - 1, 0] = downLeft;
            ret[gameObject.transform.height - 1, gameObject.transform.width - 1] = downRight;

            return ret;
        }

        public override void UpdateComponent()
        {
            if (gameObject.transform.height != sprite.GetLength(0) || gameObject.transform.width != sprite.GetLength(1)) sprite = MakeBox();
            base.UpdateComponent();
        }
    }
    public class PlayerMovement : Component
    {
        public PlayerMovement(GameObject gameObject) : base(gameObject) { }

        public override void UpdateComponent()
        {
            ConsoleKey? input = Input.GetInput();
            if (input != null)
            {
                switch (input)
                {
                    case ConsoleKey.A:
                        gameObject.transform.X--;
                        break;
                    case ConsoleKey.D:
                        gameObject.transform.X++;
                        break;
                    case ConsoleKey.W:
                        gameObject.transform.Y--;
                        break;
                    case ConsoleKey.S:
                        gameObject.transform.Y++;
                        break;
                    default:
                        break;

                }
            }
        }
    }
}
