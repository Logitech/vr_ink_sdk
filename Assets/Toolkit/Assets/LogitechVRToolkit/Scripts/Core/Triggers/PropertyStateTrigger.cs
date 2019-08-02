namespace Logitech.XRToolkit.Triggers
{
    using Logitech.XRToolkit.Core;
    using Logitech.XRToolkit.Utils;
    using System;
    using System.Reflection;
    using UnityEngine;

    /// <summary>
    /// Trigger that is valid based on the state of a public boolean property or field of a MonoBehaviour.
    /// </summary>
    [Serializable]
    public class PropertyStateTrigger : Trigger
    {
        public enum BoolEnablingType
        {
            None,
            ReturnTrueIf,
            ReturnFalseIf
        }

        public BoolEnablingType Condition;
        [HideIf("Condition", BoolEnablingType.None)]
        public Component ComponentWithPublicProperties;
        public MemberInfo BlockingElement;
        [HideInInspector]
        public string[] Choices;
        [HideInInspector]
        public int ChoiceIndex;

        public override bool IsValid()
        {
            // Update BlockingElement if it's null
            if (BlockingElement == null)
            {
                if (Choices.Length > 0 && Condition != BoolEnablingType.None)
                {
                    BlockingElement = ComponentWithPublicProperties.GetType().GetMember(Choices[ChoiceIndex])[0];
                }
                else
                {
                    BlockingElement = null;
                }
            }

            return BlockingElement != null &&
                   ((Condition == BoolEnablingType.ReturnTrueIf && (bool) GetMemberValue(BlockingElement, ComponentWithPublicProperties)) ||
                    (Condition == BoolEnablingType.ReturnFalseIf && !(bool) GetMemberValue(BlockingElement, ComponentWithPublicProperties)));
        }

        private object GetMemberValue(MemberInfo memberInfo, Component component)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Property:
                    return ((PropertyInfo)memberInfo).GetValue(component, null);
                case MemberTypes.Field:
                    return ((FieldInfo)memberInfo).GetValue(component);
                default:
                    return null;
            }
        }
    }
}
