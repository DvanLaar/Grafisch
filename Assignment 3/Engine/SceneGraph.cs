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
        public SceneNode mainNode;

        public SceneGraph()
        {

        }

        public void Render(Matrix4 camera)
        {
            mainNode.Render(Matrix4.Identity, camera);
        }

    }

    public class SceneNode
    {
        public List<SceneNode> ChildrenNodes;
        public List<Model> ChildrenModels;
        public Matrix4 Transform;

        public SceneNode()
        {
            Transform = Matrix4.Identity;
            ChildrenNodes = new List<SceneNode>();
            ChildrenModels = new List<Model>();
        }

        public void Render(Matrix4 ModelToWorld, Matrix4 WorldToScreen)
        {
            ModelToWorld = ModelToWorld * Transform;
            //WorldToScreen = WorldToScreen * Transform; //Wow, such fail, very weird
            foreach (Model model in ChildrenModels)
                model.Render(ModelToWorld,WorldToScreen);

            foreach (SceneNode node in ChildrenNodes)
                node.Render(ModelToWorld,WorldToScreen);
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
