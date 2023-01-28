using System.Text;
using UnityEngine;

[ExecuteInEditMode]
public class PrintBoneOrder : MonoBehaviour
{
    public SkinnedMeshRenderer[] SkinnedMeshRenderers;
    public bool PrintBoneOrders = false;

    void Start()
    {

    }

    void Update()
    {
        if (PrintBoneOrders && SkinnedMeshRenderers != null)
        {
            StringBuilder sb = new StringBuilder();

            int currentIndex = 0;
            int maxBoneCount = -1;

            sb.AppendLine("Parent Root Bone(s): ");
            bool loopedOnce = false;
            foreach (var smr in SkinnedMeshRenderers)
            {
                if (loopedOnce) sb.Append($",OwningObject-{GetTopParentName(smr.transform)}:SMROwner-{smr.transform.name}:RootBone-{smr.rootBone.name}");
                else sb.Append($"OwningObject-{GetTopParentName(smr.transform)}:SMROwner-{smr.transform.name}:RootBone-{smr.rootBone.name}");

                if (smr.bones != null && smr.bones.Length > 0)
                {
                    if (smr.bones.Length > maxBoneCount) maxBoneCount = smr.bones.Length;
                }
                loopedOnce = true;
            }
            sb.AppendLine();


            //print($"Parent Root Bone {parent.rootBone.name}, My Root Bone {myself.rootBone.name}");
            //print("Everything is going to be Index# : parentBone - myBone");
            while (currentIndex < maxBoneCount)
            {
                if (currentIndex > 0) sb.AppendLine();

                loopedOnce = false;
                foreach (var smr in SkinnedMeshRenderers)
                {
                    if (loopedOnce) sb.Append(",");
                    else sb.Append($"{currentIndex}: ");

                    if (currentIndex < smr.bones.Length)sb.Append(smr.bones[currentIndex].name);
                    else sb.Append("Nil");
                    loopedOnce = true;
                } 

                currentIndex++;
            }
            print(sb.ToString());
        }
        PrintBoneOrders = false;
    }

    private string GetTopParentName(Transform transform)
    {
        string topParentName = "";
        RecursiveParentSearch(transform, ref topParentName);
        return topParentName;
    }

    private void RecursiveParentSearch(Transform transform, ref string name)
    {
        if (transform.parent != null)
        {
            name = transform.parent.name;
            if (transform.parent.parent != null)
            {
                RecursiveParentSearch(transform.parent.parent, ref name);
            }
        }
        else name = transform.name;
    }
}

