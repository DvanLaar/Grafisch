using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template_P3;

namespace template_P3
{
    public class SceneGraph
    {
        private SceneNode mainNode;
        
        public SceneGraph(SceneNode mainNode) { this.mainNode = mainNode; }

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
        private List<SceneNode> ChildrenNodes;
        private List<Model> ChildrenModels;
        public Matrix4 Transform;

        public SceneNode()
        {
            Transform = Matrix4.Identity;
            ChildrenNodes = new List<SceneNode>();
            ChildrenModels = new List<Model>();
        }

        public void Render(Matrix4 ModelToWorld, Matrix4 WorldToScreen)
        {
            ModelToWorld = Transform * ModelToWorld;
            
            // FIRST render all the models
            foreach (Model model in ChildrenModels)
                model.Render(ModelToWorld, WorldToScreen);

            // SECOND recursion through the other nodes
            foreach (SceneNode node in ChildrenNodes)
                node.Render(ModelToWorld, WorldToScreen);
        }

        public void AddChildNode(SceneNode node)
        {
            ChildrenNodes.Add(node);
        }

        public void AddChildModel(Model model)
        {
            ChildrenModels.Add(model);
        }
    }
}
