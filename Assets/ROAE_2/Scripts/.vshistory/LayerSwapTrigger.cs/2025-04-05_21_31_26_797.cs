private void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag(targetTag))
    {
        Debug.Log("Intrat în trigger!");
        targetRenderer.sortingOrder = orderInLayerWhenInside;
    }
}

private void OnTriggerExit2D(Collider2D other)
{
    if (other.CompareTag(targetTag))
    {
        Debug.Log("Ieșit din trigger!");
        targetRenderer.sortingOrder = orderInLayerWhenOutside;
    }
}
