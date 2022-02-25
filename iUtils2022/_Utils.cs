using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


public static class _Utils
{
    /// <summary>
    /// Получить список ошибока с GUID и описанием ошибки
    /// </summary>
    public static string getfailId()
    {
        var registry = Autodesk.Revit.ApplicationServices.Application.GetFailureDefinitionRegistry();
        var ass = Assembly.GetAssembly(typeof(Document));
        var type = ass.GetTypes().Where(x => x.Name.StartsWith("BuiltInFailures")).FirstOrDefault();
        var members = type.GetMembers().Where(x => x.GetType().IsClass).ToList();
        var sb = new StringBuilder();
        var nullfds = new List<FailureDefinitionId>();
        foreach (var m in members)
        {
            if (m is Type t)
            {
                var props = t.GetProperties();
                foreach (var p in props)
                {
                    var value = p.GetValue(null, null);
                    var fds = registry.FindFailureDefinition(value as FailureDefinitionId);
                    if (fds == null)
                    {
                        nullfds.Add(value as FailureDefinitionId);
                    }
                    var message = fds?.GetDescriptionText();
                    var Ftype = fds?.GetSeverity();
                    sb.AppendLine($"{t.FullName.Replace("+", ".")}.{p.Name};{(value as FailureDefinitionId).Guid.ToString()};{Ftype};{message}");
                }
            }
        }
        var result = sb.ToString();
        return result;
    }


    public static Transform TransformByVectors(XYZ oldX, XYZ oldY, XYZ oldZ, XYZ oldOrigin, XYZ newX, XYZ newY, XYZ newZ, XYZ newOrigin)
    {

        // [new vector] = [transform]*[old vector]
        // [3x1] = [3x4] * [4x1]
        // 
        // [v'x]   [ i*i'  j*i'  k*i'  translationX' ]   [vx]
        // [v'y] = [ i*j'  j*j'  k*j'  translationY' ] * [vy]
        // [v'z]   [ i*k'  j*k'  k*k'  translationZ' ]   [vz]
        // [1 ]
        Transform t = Transform.Identity;

        double xx = oldX.DotProduct(newX);
        double xy = oldX.DotProduct(newY);
        double xz = oldX.DotProduct(newZ);

        double yx = oldY.DotProduct(newX);
        double yy = oldY.DotProduct(newY);
        double yz = oldY.DotProduct(newZ);

        double zx = oldZ.DotProduct(newX);
        double zy = oldZ.DotProduct(newY);
        double zz = oldZ.DotProduct(newZ);

        t.BasisX = new XYZ(xx, xy, xz);
        t.BasisY = new XYZ(yx, yy, yz);
        t.BasisZ = new XYZ(zx, zy, zz);

        // The movement of the origin point 
        // in the old coordinate system

        XYZ translation = newOrigin - oldOrigin;

        // Convert the translation into coordinates 
        // in the new coordinate system

        double translationNewX = xx * translation.X
            + yx * translation.Y
            + zx * translation.Z;

        double translationNewY = xy * translation.X
            + yy * translation.Y
            + zy * translation.Z;

        double translationNewZ = xz * translation.X
            + yz * translation.Y
            + zz * translation.Z;

        t.Origin = new XYZ(-translationNewX, -translationNewY, -translationNewZ);

        return t;
    }

    public static Transform PlanarFaceTransform(PlanarFace face)
    {
        return TransformByVectors(XYZ.BasisX, XYZ.BasisY, XYZ.BasisZ, XYZ.Zero, face.XVector, face.YVector, face.FaceNormal, face.Origin);
    }

    public const double fyt = 304.8;
    /// <summary>
    /// Выводит окно TaskDialog
    /// </summary>
    /// <param name="s"></param>
    public static void Show(string s)
    {
        MessageBox.Show(s, "Olimproekt.bim");
    }

    /// <summary>
    /// Выводит окно TaskDialog
    /// </summary>
    /// <param name="s"></param>
    public static void Show(int s)
    {
        MessageBox.Show(s.ToString(), "Olimproekt.bim");
        //TaskDialog.Show("autoCADnet", s.ToString());
    }

    /// <summary>
    /// Выводит окно TaskDialog
    /// </summary>
    /// <param name="s"></param>
    public static void Show(double s)
    {
        MessageBox.Show(s.ToString(), "Olimproekt.bim");
        //TaskDialog.Show("autoCADnet", s.ToString());
    }

    /// <summary>
    /// Выводит окно TaskDialog
    /// </summary>
    /// <param name="s"></param>
    public static void Show(bool s)
    {
        MessageBox.Show(s.ToString(), "Olimproekt.bim");
        // TaskDialog.Show("autoCADnet", s.ToString());
    }

    /// <summary>
    /// Выводит окно TaskDialog
    /// </summary>
    /// <param name="s"></param>
    public static void Show(XYZ s)
    {
        MessageBox.Show(s.X.ToString() + " | " + s.Y.ToString() + " | " + s.Z.ToString(), "Olimproekt.bim");
    }

    //---

    /// <summary>
    /// Преобразует Curve в вектор нормали, например (1;0;0)
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public static XYZ Normal(Curve c)
    {
        try
        {
            XYZ value = new XYZ();
            XYZ p1 = c.GetEndPoint(0);
            XYZ p2 = c.GetEndPoint(1);
            value = new XYZ((p2.X - p1.X) / c.Length, (p2.Y - p1.Y) / c.Length, (p2.Z - p1.Z) / c.Length);

            return value;
        }
        catch (Exception ex)
        {
            TaskDialog.Show("Warring in method _Utils.Normal", ex.Message);
        }
        return new XYZ();
    }

    /// <summary>
    /// Преобразует Edge в вектор нормали, например (1;0;0)
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public static XYZ Normal(Edge c)
    {
        try
        {
            XYZ value = new XYZ();
            XYZ p1 = c.AsCurve().GetEndPoint(0);
            XYZ p2 = c.AsCurve().GetEndPoint(1);
            value = new XYZ((p2.X - p1.X) / c.AsCurve().Length, (p2.Y - p1.Y) / c.AsCurve().Length, (p2.Z - p1.Z) / c.AsCurve().Length);

            return value;
        }
        catch (Exception ex)
        {
            TaskDialog.Show("Warring in method _Utils.Normal", ex.Message);
        }
        return new XYZ();
    }


    /// <summary>
    /// Проверяет находится ли Edge в плоскости XOY
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public static bool IsHor(Edge e)
    {
        XYZ value = Normal(e);
        if (value.Z >= -1 / fyt && value.Z <= 1 / fyt)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    /// <summary>
    /// Проверяет находится ли Curve в плоскости XOY
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public static bool IsHor(Curve e)
    {
        XYZ value = Normal(e);
        if (value.Z >= -1 / fyt && value.Z <= 1 / fyt)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Проверяет вертикальность Edge
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public static bool IsVert(Edge e)
    {
        XYZ value = Normal(e);
        if (value.Z >= 0.9 / fyt && value.Z <= 1.1 / fyt)
        {
            _Utils.Show(value.Z);
            return true;
        }
        else if (value.Z >= -1.1 / fyt && value.Z <= -0.9 / fyt)
        {
            _Utils.Show(value.Z);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Проверяет вертикальность Curve
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public static bool IsVert(Curve e)
    {
        XYZ value = Normal(e);
        if (value.Z >= -1 / fyt && value.Z <= 1 / fyt)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    /// <summary>
    /// Проверяет ортогональность двух вектров в плоскости XOY, и равны ли они нулю
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    public static bool IsVectorsPerpendicularInXOY(XYZ v1, XYZ v2)
    {
        double len1, len2;
        len1 = v1.GetLength();
        len2 = v2.GetLength();
        if (len1 == 0 || len2 == 0)
        {
            Show("Ошибка метода IsVectorsPerpendicularInXOY, один из входных векторов нулевой!");
            return false;
        }

        double ugol = v1.AngleTo(v2);
        double grad = Math.PI / 180;
        double error = 1000000000;
        if (ugol <= Math.PI / 2 + grad / error && ugol >= Math.PI / 2 - grad / error)
            return true;

        return false;
    }

    /// <summary>
    /// Проверяет ортогональность двух Curve
    /// </summary>
    /// <param name="c1"></param>
    /// <param name="c2"></param>
    /// <returns></returns>
    public static bool IsCurvesPerpendicularInXOY(Curve c1, Curve c2)
    {
        XYZ v1 = Normal(c1);
        XYZ v2 = Normal(c2);
        bool result = IsVectorsPerpendicularInXOY(v1, v2);
        return result;
    }

    //---

    /// <summary>
    /// Получает ребра элемента
    /// </summary>
    /// <param name="elem"></param>
    /// <returns></returns>
    static public List<Edge> GetEdgesofElem(Element elem)
    {
        List<Edge> edges = new List<Edge>();
        GeometryElement geomel = elem.get_Geometry(new Options());

        foreach (GeometryObject geomob in geomel)
        {
            Solid solid = geomob as Solid;
            if (null != solid)
            {
                foreach (Edge edge in solid.Edges)
                {
                    edges.Add(edge);
                }
            }
        }
        return edges;
    }

    /// <summary>
    /// Получает грани элемента
    /// </summary>
    /// <param name="elem"></param>
    /// <returns></returns>
    static public List<Face> GetFacesofElem(Element elem)
    {
        Options op = new Options();
        op.ComputeReferences = true;
        op.View = elem.Document.ActiveView;

        List<Face> faces = new List<Face>();
        GeometryElement geomel = elem.get_Geometry(op);

        foreach (GeometryObject geomob in geomel)
        {
            Solid solid = geomob as Solid;
            if (null != solid)
            {
                foreach (Face edge in solid.Faces)
                {
                    faces.Add(edge);
                }
            }
        }
        return faces;
    }


    //---

    /// <summary>
    /// Преобразует футы в миллиметры
    /// </summary>
    /// <param name="d"></param>
    /// <returns></returns>
    public static double FytTomm(double d)
    {
        return UnitUtils.Convert(d, UnitTypeId.Feet, UnitTypeId.Millimeters);
    }
    /// <summary>
    /// Преобразует миллиметры в футы
    /// </summary>
    /// <param name="d"></param>
    /// <returns></returns>
    public static double mmToFyt(double d)
    {
        return UnitUtils.Convert(d, UnitTypeId.Millimeters, UnitTypeId.Feet);
    }

    public static double mmToFyt2(this double d)
    {
        return UnitUtils.Convert(d, UnitTypeId.Millimeters, UnitTypeId.Feet);
    }
    public static double FytTomm2(this double d)
    {
        return UnitUtils.Convert(d, UnitTypeId.Feet, UnitTypeId.Millimeters);
    }
    public static double mmToFyt2(this int d)
    {
        return UnitUtils.Convert(d, UnitTypeId.Millimeters, UnitTypeId.Feet);

    }
    public static double FytTomm2(this int d)
    {
        return UnitUtils.Convert(d, UnitTypeId.Feet, UnitTypeId.Millimeters);
    }

    public static double FytAreatoMeters(double a)
    {
        return UnitUtils.Convert(a, UnitTypeId.SquareFeet, UnitTypeId.SquareMeters);
    }
    public static double MetersAreatoFyt(double a)
    {
        return UnitUtils.Convert(a, UnitTypeId.SquareMeters, UnitTypeId.SquareFeet);
    }
    //---


    /// <summary>
    /// Возващает список типов арматуры документа
    /// </summary>
    /// <param name="doc"></param>
    /// <returns></returns>
    public static List<RebarBarType> GetBarTypeListFormDoc(Document doc)
    {
        List<RebarBarType> rebarTypes = new List<RebarBarType>();
        FilteredElementCollector fec = new FilteredElementCollector(doc);
        fec.OfClass(typeof(RebarBarType));
        rebarTypes = fec.Cast<RebarBarType>().ToList<RebarBarType>();
        return rebarTypes;
    }

    /// <summary>
    /// Возващает список форм арматуры документа
    /// </summary>
    /// <param name="doc"></param>
    /// <returns></returns>
    public static List<RebarShape> GetBarShapeListFormDoc(Document doc)
    {
        List<RebarShape> rebarShapes = new List<RebarShape>();
        FilteredElementCollector fec = new FilteredElementCollector(doc);
        fec.OfClass(typeof(RebarShape));
        rebarShapes = fec.Cast<RebarShape>().ToList<RebarShape>();
        return rebarShapes;
    }


    //--

    /// <summary>
    /// Возвращает минимальную коориданату Z из списка Curve
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public static double GetMinZPointFromCurveList(List<Curve> list)
    {
        double value;
        List<double> mass = new List<double>();
        foreach (Curve item in list)
        {
            mass.Add(item.GetEndPoint(0).Z);
            mass.Add(item.GetEndPoint(1).Z);
        }
        mass.Sort();
        value = mass.First();
        return value;
    }
    public static double GetMinZPointFromCurveList(List<Edge> list)
    {
        double value;

        List<double> mass = new List<double>();
        foreach (Edge item in list)
        {
            mass.Add(item.AsCurve().GetEndPoint(0).Z);
            mass.Add(item.AsCurve().GetEndPoint(1).Z);
        }

        mass.Sort();
        value = mass.First();
        return value;
    }



    /// <summary>
    /// Возвращает максимальную коориданату Z из списка Curve
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public static double GetMaxZPointFromCurveList(List<Curve> list)
    {
        double value;
        List<double> mass = new List<double>();
        foreach (Curve item in list)
        {
            mass.Add(item.GetEndPoint(0).Z);
            mass.Add(item.GetEndPoint(1).Z);
        }
        mass.Sort();
        value = mass.Last();
        return value;
    }
    public static double GetMaxZPointFromCurveList(List<Edge> list)
    {
        double value;
        List<double> mass = new List<double>();
        foreach (Edge item in list)
        {
            mass.Add(item.AsCurve().GetEndPoint(0).Z);
            mass.Add(item.AsCurve().GetEndPoint(1).Z);
        }
        mass.Sort();
        value = mass.Last();
        return value;
    }

    /// <summary>
    /// d1 сравниваемое значение, которое должно находится в пределах d2+- error, error задается в методе (равно 1 мм)
    /// </summary>
    /// <param name="d1"></param>
    /// <param name="d2"></param>
    /// <returns></returns>
    public static bool EqualCoordinates(double d1, double d2)
    {
        double error = 1 / fyt;
        if (d1 <= d2 + error && d1 >= d2 - error)
        {
            return true;
        }

        return false;
    }

    public static XYZ RoundVector(XYZ vector)
    {
        double x = vector.X;
        double y = vector.Y;
        double z = vector.Z;

        const double value = 1e-12;

        if (x < value)
            x = 0;
        if (y < value)
            y = 0;
        if (z < value)
            z = 0;
        return new XYZ(x, y, z);
    }

    public static double RoundUp(double a, double poraidok)
    {
        double i = Math.Truncate(a / poraidok);
        i++;
        i = i * poraidok;
        return i;
    }


    public static bool MoreLess(this XYZ p1, XYZ p2)
    {
        return p1.X > p2.X & p1.Y > p2.Y & p1.Z > p2.Z;
    }


    public static XYZ NormalToWall(Wall c)
    {
        Document doc = c.Document;
        Autodesk.Revit.DB.View v = doc.ActiveView;
        double or = v.ViewDirection.Z;
        if (or != 1)
        {
            _Utils.Show("Ошибка метоад NormalToWall, ViewDirection.Z !=1");
            return null;
        }
        LocationCurve lc = c.Location as LocationCurve;
        XYZ n = Normal(lc.Curve);
        XYZ xVec = new XYZ(1, 0, 0);
        double angel = xVec.AngleTo(n);
        angel = angel + Math.PI / 2;
        double x = Math.Cos(angel);
        double y = Math.Sin(angel);
        return new XYZ(x, y, 0);
    }

    public static Line OffsetCurve(Line c, XYZ vector, double d)
    {
        vector.Multiply(d);
        XYZ p1 = c.GetEndPoint(0);
        XYZ p2 = c.GetEndPoint(1);
        XYZ np1 = new XYZ(p1.X + Math.Cos(vector.X), p1.Y + Math.Sin(vector.Y), p1.Z + vector.Z);
        XYZ np2 = new XYZ(p2.X + Math.Cos(vector.X), p2.Y + Math.Sin(vector.Y), p2.Z + vector.Z);
        Line l = Line.CreateBound(np1, np2);
        return l;
    }

    public static StringBuilder ReadFile(string file)
    {
        var sb = new StringBuilder();
        var rf2 = File.OpenRead(file);
        using (rf2)
        {
            byte[] b = new byte[1024000];
            UTF8Encoding temp = new UTF8Encoding(true);

            while (rf2.Read(b, 0, b.Length) > 0)
            {
                sb.Append(temp.GetString(b));
            }
        }

        return sb;
    }
    
}

