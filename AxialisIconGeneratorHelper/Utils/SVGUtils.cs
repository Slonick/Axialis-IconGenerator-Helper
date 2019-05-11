#region Usings

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;

#endregion

namespace AxialisIconGeneratorHelper.Utils
{
    public static class SvgUtils
    {
        #region Internal Fields

        internal static XNamespace NsDef = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        internal static XmlNamespaceManager NsManager = new XmlNamespaceManager(new NameTable());
        internal static XNamespace Nsx = "http://schemas.microsoft.com/winfx/2006/xaml";

        #endregion

        #region Static Constructors

        static SvgUtils()
        {
            NsManager.AddNamespace("defns", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            NsManager.AddNamespace("x", "http://schemas.microsoft.com/winfx/2006/xaml");
        }

        #endregion

        #region Public Methods

        public static DrawingGroup ConvertToDrawingGroup(string svgContent)
        {
            var settings = new WpfDrawingSettings
            {
                IncludeRuntime = false,
                TextAsGeometry = false,
                OptimizePath = true
            };

            var converter = new FileSvgReader(settings);
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(svgContent)))
            {
                return converter.Read(stream);
            }
        }

        public static string ConvertToXaml(string svgContent)
        {
            var settings = new WpfDrawingSettings
            {
                IncludeRuntime = false,
                TextAsGeometry = false,
                OptimizePath = true
            };

            var converter = new FileSvgReader(settings);

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(svgContent)))
            {
                var dg = converter.Read(stream);
                SetSizeToGeometries(dg);
                RemoveObjectNames(dg);

                return SvgObjectToXaml(new DrawingImage(dg), settings.IncludeRuntime, "Icon");
            }
        }

        #endregion

        #region Internal Methods

        internal static void BeautifyDrawingElement(XElement drawingElement, string name)
        {
            InlineClipping(drawingElement);
            RemoveCascadedDrawingGroup(drawingElement);
            CollapsePathGeometries(drawingElement);
            SetDrawingElementXKey(drawingElement, name);
        }

        internal static void CollapsePathGeometries(XElement drawingElement)
        {
            var pathGeometries = drawingElement.Descendants(NsDef + "PathGeometry").ToArray();
            foreach (var pathGeometry in pathGeometries)
            {
                if (pathGeometry.Parent?.Parent == null || pathGeometry.Parent.Parent.Name.LocalName != "GeometryDrawing") continue;

                //check if only FillRule and Figures is available
                var attrNames = pathGeometry.Attributes().Select(a => a.Name.LocalName).ToList();
                if (attrNames.Count > 2 || !attrNames.Contains("Figures") || !attrNames.Contains("FillRule") && attrNames.Count != 1) continue;
                var sFigures = pathGeometry.Attribute("Figures")?.Value;
                var fillRuleAttr = pathGeometry.Attribute("FillRule");
                if (fillRuleAttr != null)
                {
                    if (fillRuleAttr.Value == "Nonzero")
                        sFigures = "F1 " + sFigures; //Nonzero
                    else
                        sFigures = "F0 " + sFigures; //EvenOdd
                }

                if (sFigures != null) pathGeometry.Parent.Parent.Add(new XAttribute("Geometry", sFigures));
                pathGeometry.Parent.Remove();
            }
        }

        internal static XElement GetClipElement(XElement drawingGroupElement, out Rect rect)
        {
            rect = default;
            var clipElement = drawingGroupElement?.XPathSelectElement(".//defns:DrawingGroup.ClipGeometry", NsManager);
            var rectangleElement = clipElement?.Element(NsDef + "RectangleGeometry");
            var rectAttr = rectangleElement?.Attribute("Rect");
            if (rectAttr == null) return null;
            rect = Rect.Parse(rectAttr.Value);
            return clipElement;
        }

        internal static IEnumerable<PathGeometry> GetPathGeometries(Drawing drawing)
        {
            var result = new List<PathGeometry>();

            void HandleDrawing(Drawing aDrawing)
            {
                switch (aDrawing)
                {
                    case DrawingGroup drawingGroup:
                    {
                        foreach (var d in drawingGroup.Children) HandleDrawing(d);

                        break;
                    }

                    case GeometryDrawing geometryDrawing:
                    {
                        var gd = geometryDrawing;
                        var geometry = gd.Geometry;
                        if (geometry is PathGeometry pathGeometry) result.Add(pathGeometry);

                        break;
                    }
                }
            }

            HandleDrawing(drawing);

            return result;
        }

        internal static Size? GetSizeFromDrawingGroup(DrawingGroup drawingGroup)
        {
            var subGroup = drawingGroup?.Children.OfType<DrawingGroup>()
                                       .FirstOrDefault(c => c.ClipGeometry != null);

            return subGroup?.ClipGeometry.Bounds.Size;
        }

        internal static void InlineClipping(XElement drawingElement)
        {
            var clipElement = GetClipElement(drawingElement, out var clipRect);
            if (clipElement == null || clipElement.Parent?.Name.LocalName != "DrawingGroup") return;
            clipElement.Parent.Add(new XAttribute("ClipGeometry", string.Format(CultureInfo.InvariantCulture, "M{0},{1} V{2} H{3} V{0} H{1} Z", clipRect.Left, clipRect.Top, clipRect.Bottom, clipRect.Right)));
            clipElement.Remove();
        }

        internal static void RemoveCascadedDrawingGroup(XElement drawingElement)
        {
            var drawingGroups = drawingElement.DescendantsAndSelf(NsDef + "DrawingGroup");
            foreach (var drawingGroup in drawingGroups)
            {
                var elems = drawingGroup.Elements().ToList();
                if (elems.Count != 1 || elems[0].Name.LocalName != "DrawingGroup") continue;
                var subGroup = elems[0];

                var subAttrNames = subGroup.Attributes().Select(a => a.Name);
                var attrNames = drawingGroup.Attributes().Select(a => a.Name);
                if (subAttrNames.Intersect(attrNames).Any())
                    return;
                drawingGroup.Add(subGroup.Attributes());
                drawingGroup.Add(subGroup.Elements());
                subGroup.Remove();
            }
        }

        internal static string RemoveNamespaceDeclarations(string xml)
        {
            xml = xml.Replace(" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"", "");
            xml = xml.Replace(" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"", "");
            return xml;
        }

        internal static void RemoveObjectNames(DrawingGroup drawingGroup)
        {
            if (drawingGroup.GetValue(FrameworkElement.NameProperty) != null)
                drawingGroup.SetValue(FrameworkElement.NameProperty, null);
            foreach (var groupChild in drawingGroup.Children)
            {
                if (!(groupChild is DependencyObject child)) continue;

                if (child.GetValue(FrameworkElement.NameProperty) != null) child.SetValue(FrameworkElement.NameProperty, null);
                if (child is DrawingGroup group) RemoveObjectNames(group);
            }
        }

        internal static void SetDrawingElementXKey(XElement drawingElement, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;
            var attributes = drawingElement.Attributes().ToList();
            attributes.Insert(0, new XAttribute(Nsx + "Key", name));
            drawingElement.ReplaceAttributes(attributes);
        }

        internal static void SetSizeToGeometries(DrawingGroup dg)
        {
            var size = GetSizeFromDrawingGroup(dg);
            if (!size.HasValue) return;

            var geometries = GetPathGeometries(dg).ToList();
            geometries.ForEach(g => SizeGeometry(g, size.Value));
        }

        internal static void SizeGeometry(PathGeometry pg, Size size)
        {
            if (!(size.Height > 0) || !(size.Height > 0)) return;

            PathFigure[] sizeFigures =
            {
                new PathFigure(new Point(size.Width, size.Height), Enumerable.Empty<PathSegment>(), true),
                new PathFigure(new Point(0, 0), Enumerable.Empty<PathSegment>(), true)
            };

            var newGeo = new PathGeometry(sizeFigures.Concat(pg.Figures), pg.FillRule, null);
            pg.Clear();
            pg.AddGeometry(newGeo);
        }

        internal static string SvgObjectToXaml(object obj, bool includeRuntime, string name)
        {
            var xamlUntidy = WpfObjToXaml(obj, includeRuntime);

            var doc = XDocument.Parse(xamlUntidy);
            BeautifyDrawingElement(doc.Root, name);
            var xamlWithNamespaces = doc.ToString();

            var xamlClean = RemoveNamespaceDeclarations(xamlWithNamespaces);
            return xamlClean;
        }

        internal static string WpfObjToXaml(object wpfObject, bool includeRuntime)
        {
            var writer = new XmlXamlWriter(new WpfDrawingSettings {IncludeRuntime = includeRuntime});
            var xaml = writer.Save(wpfObject);
            return xaml;
        }

        #endregion
    }
}