using _3D_Engine.Classes.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Engine.Classes.Scenes
{
    public static class SceneManager
    {
        public static Scene? CurrentScene;
        public static List<WorldObject> RenderedEntities = new List<WorldObject>();

        public static void LoadScene(Scene newScene)
        {
            CurrentScene?.destroyScene();
            CurrentScene = newScene;
            CurrentScene.createScene();
        }
    }

}
