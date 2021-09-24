using System.Collections.Generic;
using System.Linq;

namespace OpenRCT2.Content
{
    public class ValidatedForm : ValidatedValue
    {
        private readonly List<ValidatedValue> _children = new List<ValidatedValue>();

        public void AddChildren(params ValidatedValue[] children)
        {
            _children.AddRange(children);
        }

        public override void ResetValidation()
        {
            base.ResetValidation();
            foreach (var child in _children)
            {
                child.ResetValidation();
            }
        }

        public bool AreAllChildrenValid => !_children.Any(x => x.IsValid == false);
    }
}
