using UnityEngine;

public interface IRadiusTriggerListener
{
    void OnRadiusTriggerEnter2D(Collider2D collision);
    void OnRadiusTriggerStay2D(Collider2D collision);
    void OnRadiusTriggerExit2D(Collider2D collision);
}