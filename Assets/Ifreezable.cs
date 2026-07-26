using UnityEngine;

// Movement scripts that use coroutines should implement this so the
// Electric Eel (or anything else) can pause them without killing their
// coroutine state entirely (StopAllCoroutines() can't be resumed cleanly).
public interface IFreezable
{
    void OnFrozen();
    void OnUnfrozen();
}