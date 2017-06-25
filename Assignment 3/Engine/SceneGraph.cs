using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rasterizer;

namespace rasterizer
{
    public class SceneGraph
    {
        private SceneNode mainNode;

        public SceneGraph(SceneNode mainNode)
        {
            this.mainNode = mainNode;
        }

        /// <summary>
        /// Render the whole scene using the given camera matrix starting in the main node
        /// </summary>
        /// <param name="camera"></param>
        public void Render(Matrix4 camera)
        {
            mainNode.Render(Matrix4.Identity, camera);
        }
    }

    public class SceneNode
    {
        private List<SceneNode> childrenNodes;
        private List<Model> childrenModels;
        public Matrix4 Transformation;

        public SceneNode()
        {
            Transformation = Matrix4.Identity;
            childrenNodes = new List<SceneNode>();
            childrenModels = new List<Model>();
        }

        public void Render(Matrix4 ModelToWorld, Matrix4 WorldToScreen)
        {
            ModelToWorld = ModelToWorld * Transformation;

            // FIRST render all the models
            foreach (Model model in childrenModels)
                model.Render(ModelToWorld, WorldToScreen);

            // go deeper in the recursion of the scene node tree
            foreach (SceneNode node in childrenNodes)
                node.Render(ModelToWorld, WorldToScreen);
        }

        public void AddChildNode(SceneNode node)
        {
            childrenNodes.Add(node);
        }

        public void AddChildModel(Model model)
        {
            childrenModels.Add(model);
        }
    }
}
