using UnityEngine;
using UnityEngine.UI;

namespace App.Presentation.FigureTray
{
    public sealed class FigureBlockCellView : MonoBehaviour
    {
        [SerializeField] private Image _image;

        public void SetImageToDefault()
        {
            _image.color = Color.white;
        }
    }
}