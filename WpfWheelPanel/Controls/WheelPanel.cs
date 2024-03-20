using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace WpfWheelPanel.Controls;


public class WheelPanel : Panel
{
    #region Dependency Properties

    const double DefaultSpacing = 10.0;
    const double DefaultMinInnerRadius = 50.0;
    const double DefaultItemSize = 20.0;
    const double DefaultAngleOffset = 0.0;


    public static readonly DependencyProperty SpacingProperty =
        DependencyProperty.RegisterAttached(
            nameof(Spacing), typeof(double), typeof(WheelPanel),
            new FrameworkPropertyMetadata(
                DefaultSpacing,
                FrameworkPropertyMetadataOptions.Inherits));


    public static readonly DependencyProperty MinInnerRadiusProperty =
        DependencyProperty.RegisterAttached(
            nameof(MinInnerRadius), typeof(double), typeof(WheelPanel),
            new FrameworkPropertyMetadata(
                DefaultMinInnerRadius,
                FrameworkPropertyMetadataOptions.Inherits));


    public static readonly DependencyProperty ItemSizeProperty =
        DependencyProperty.RegisterAttached(
            nameof(ItemSize), typeof(double), typeof(WheelPanel),
            new FrameworkPropertyMetadata(
                DefaultItemSize,
                FrameworkPropertyMetadataOptions.Inherits));


    public static readonly DependencyProperty AngleOffsetProperty =
        DependencyProperty.RegisterAttached(
            nameof(AngleOffset), typeof(double), typeof(WheelPanel),
            new FrameworkPropertyMetadata(
                DefaultAngleOffset,
                FrameworkPropertyMetadataOptions.Inherits));


    public static readonly DependencyPropertyKey OuterRadiusPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(CalculateOuterRadius), typeof(double),
            typeof(WheelPanel),
            new FrameworkPropertyMetadata(
                0.0,
                FrameworkPropertyMetadataOptions.Inherits));


    public static readonly DependencyProperty OuterRadiusProperty =
        OuterRadiusPropertyKey.DependencyProperty;


    public static double GetSpacing(DependencyObject element) =>
        (double)element.GetValue(SpacingProperty);


    public static void SetSpacing(DependencyObject element, double value) =>
        element.SetValue(SpacingProperty, value);


    public double Spacing
    {
        get => GetSpacing(this);
        set => SetSpacing(this, value);
    }


    public static double GetMinInnerRadius(DependencyObject element) =>
        (double)element.GetValue(MinInnerRadiusProperty);


    public static void SetMinInnerRadius(DependencyObject element, double value) =>
        element.SetValue(MinInnerRadiusProperty, value);


    public double MinInnerRadius
    {
        get => GetMinInnerRadius(this);
        set => SetMinInnerRadius(this, value);
    }


    public static double GetItemSize(DependencyObject element) =>
        (double)element.GetValue(ItemSizeProperty);


    public static void SetItemSize(DependencyObject element, double value) =>
        element.SetValue(ItemSizeProperty, value);


    public double ItemSize
    {
        get => GetItemSize(this);
        set => SetItemSize(this, value);
    }


    public double OuterRadius
    {
        get => (double)this.GetValue(OuterRadiusProperty);
        set => this.SetValue(OuterRadiusPropertyKey, value);
    }


    public static double GetAngleOffset(DependencyObject element) =>
        (double)element.GetValue(AngleOffsetProperty);


    public static void SetAngleOffset(DependencyObject element, double value) =>
        element.SetValue(AngleOffsetProperty, value);


    public double AngleOffset
    {
        get => GetAngleOffset(this);
        set => SetAngleOffset(this, value);
    }

    #endregion


    protected override Size MeasureOverride(Size availableSize)
    {
        MeasureChildren();
        return CalculateSize();


        void MeasureChildren()
        {
            var itemSize = this.ItemSizeAsSize();
            foreach (UIElement child in this.Children)
            {
                child.Measure(itemSize);
            }
        }


        Size CalculateSize()
        {
            var radius = this.CalculateOuterRadius();
            return new Size(radius * 2, radius * 2);
        }
    }


    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (var (child, (rect, angle)) in this.Children.OfType<UIElement>()
            .Zip(this.ChildrenRectAndAngle(finalSize)))
        {
            child.Arrange(rect);
            SetAngleOffset(child, RadToDeg(angle));
        }

        return finalSize;
    }


    [Pure] IEnumerable<(Rect rect, double angle)> ChildrenRectAndAngle(Size panelSize)
    {
        var innerRadius = this.InnerRadius();

        var itemSize = this.ItemSize;

        // circle that goes through item center points
        var middleRadius = innerRadius + itemSize / 2;
        var scale = new Matrix(
            middleRadius, 0,
            0, middleRadius,
            0, 0);

        var translation = new Matrix(
            1, 0,
            0, 1,
            -itemSize / 2, -itemSize / 2);

        // angle between vectors that go through adjacent items center points
        var stepAngle = -(itemSize + this.Spacing) / innerRadius;
        var angleOffset = DegToRad(this.AngleOffset);

        var center = new Vector(panelSize.Width / 2, panelSize.Height / 2);
        var topPoint = new Point(0, -1);

        var itemSizeAsSize = this.ItemSizeAsSize();
        for (var i = 0; i < this.Children.Count; ++i)
        {
            var itemAngle = angleOffset + stepAngle * i;
            var cos = Math.Cos(itemAngle);
            var sin = Math.Sin(itemAngle);
            var rotation = new Matrix(
                cos, -sin,
                sin, cos, 0.0, 0.0);

            var itemCenter = center + topPoint * scale * rotation * translation;
            var itemRect = new Rect(itemCenter, itemSizeAsSize);
            yield return (itemRect, itemAngle);
        }
    }


    Size ItemSizeAsSize()
    {
        var itemSize = this.ItemSize;
        return new Size(itemSize, itemSize);
    }


    double InnerRadius()
    {
        var innerCircleLength = this.Children.Count * (this.ItemSize + this.Spacing);
        var innerRadius = innerCircleLength / (2 * Math.PI);
        innerRadius = Math.Max(innerRadius, this.MinInnerRadius);

        return innerRadius;
    }


    double CalculateOuterRadius()
    {
        var outerRadius = this.InnerRadius() + this.ItemSize * 2;
        this.OuterRadius = outerRadius;
        return outerRadius;
    }


    static double DegToRad(double deg) => Math.PI / 180 * deg;
    static double RadToDeg(double rad) => 180 / Math.PI * rad;
}