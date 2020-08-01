using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Sharp.UI;
using Sharp.Core;
using Sharp.Core.Variables;
using Sharp.Managers;
using DG.Tweening;

namespace Sharp.Editor
{
    public class EditorHighlight : MonoBehaviour
    {
        private void Awake() =>
            animation = gameObject.AddComponent<TweenContainer>().Init
            (
                DOTween.Sequence().Insert(frameSprite.DOFade(frameSprite.color.a * 2, Constants.Time)),
                DOTween.Sequence().Insert(selectionSprite.DOFade(0, Constants.Time))
            );

        private void Update()
        {
            TargetGrid();
            DisplayInfo();

            if (!UIUtility.IsOverUI)
                ProcessMouse();
        }

        #region gameplay

        [Header("Gameplay")]
        private bool dragging;
        private bool Dragging
        {
            get => dragging;
            set
            {
                dragging = value;
                animation[0].Play(!Dragging);
            }
        }

        private int layer;
        public int Layer
        {
            get => layer;
            set
            {
                layer = value;
                ClearInput();
            }
        }

        public string SourceName { get; set; }

        #region targeting

        [SerializeField]
        private KeyVariable copyKey;
        [SerializeField]
        private EditorProperties propertiesManager;

        private GameObject selected;
        private GameObject Selected
        {
            get => selected;
            set
            {
                selected = value;

                propertiesManager.Load(Selected);
                animation[1].Play(Selected);
            }
        }

        private GameObject target;
        private bool copied;

        private void TargetGrid()
        {
            Vector3 position = Vector3Int.RoundToInt(EditorGrid.Clamp(EditorGrid.MousePosition()));
            Collider2D collider = Physics2D.OverlapPoint(position, 1 << Layer);

            if (Dragging)
            {
                if (!collider)
                {
                    if (Keyboard.current[copyKey].isPressed && !copied && target.name != "Player" && target.name != "Exit")
                    {
                        copied = true;

                        GameObject copy = LevelManager.AddInstance(target.name, target.transform.position, true);
                        LevelManager.CopyProperties(target, copy);
                    }

                    target.transform.position = position;
                }
            }
            else
                target = collider?.gameObject;

            frameSprite.transform.position = target ? target.transform.position : position;
            if (Selected)
                selectionSprite.transform.position = Selected.transform.position;
        }

        #endregion

        #region info

        [Space(10)]
        [SerializeField]
        private Text positionText;
        [SerializeField]
        private Text layerText;
        [SerializeField]
        private Text objectText;

        private void DisplayInfo()
        {
            positionText.text = $"{frameSprite.transform.position.x}, {frameSprite.transform.position.y}";
            layerText.text = LayerMask.LayerToName(Layer);
            objectText.text = target ? $"<b>{target.name}</b>" : SourceName;
        }

        #endregion

        private void ProcessMouse()
        {
            if (target)
            {
                if (!Dragging && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    Dragging = true;
                    Selected = Selected == target ? null : target;
                }
                else if (Dragging && !Mouse.current.leftButton.isPressed)
                {
                    LevelManager.UpdateInstance(target);

                    Dragging = false;
                    copied = false;
                }
                else if (Mouse.current.rightButton.isPressed && target.name != "Player" && target.name != "Exit")
                {
                    Dragging = false;
                    if (Selected == target)
                        Selected = null;

                    LevelManager.RemoveInstance(target);
                    target = null;
                }
            }
            else if (Mouse.current.leftButton.isPressed)
                LevelManager.AddInstance(SourceName, frameSprite.transform.position, true);
        }

        public void ClearInput()
        {
            Dragging = false;
            copied = false;
            Selected = null;
        }

        #endregion

        #region animation

        [Header("Animation")]
        [SerializeField]
        private SpriteRenderer frameSprite;
        [SerializeField]
        private SpriteRenderer selectionSprite;

        private new TweenContainer animation;

        #endregion
    }
}