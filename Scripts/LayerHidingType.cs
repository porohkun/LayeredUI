namespace LayeredUI
{
    /// <summary>
    /// Степень прозрачности слоя. В каких случаях слой скрывает нижние слои.
    /// </summary>
    public enum LayerHidingType
    {
        /// <summary>
        /// Значение по умолчанию. Слой скрывает все нижние слои с типом видимости "по умолчанию".
        /// </summary>
        Default = 0,
        /// <summary>
        /// Слой не скрывает нижние слои.
        /// </summary>
        Transparent = -1,
        /// <summary>
        /// Слой скрывает все нижние слои.
        /// </summary>
        HideAll = 1
    }
}
