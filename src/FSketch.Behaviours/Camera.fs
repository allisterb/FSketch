﻿namespace FSketch.Behaviours

module Camera =
    let rec internal takeSnapshot eval (shapes:Shapes, viewport: Viewport option) : FSketch.Frame =
        let evalTransformMatrix (TransformMatrix((m11, m12), (m21, m22), (mx, my))) =
            FSketch.TransformMatrix ((eval m11, eval m12), (eval m21, eval m22), (eval mx, eval my))

        let evalRefSpace (refSpace:RefSpace) : FSketch.RefSpace = {
            transform = evalTransformMatrix refSpace.transform
            z = eval refSpace.z }

        let evalVector (Vector(x, y)) = FSketch.Vector(eval x, eval y)

        let evalPathPart (pathPart:PathPart) : FSketch.PathPart =
            match pathPart with
            | Line v -> FSketch.Line (evalVector v)
            | Bezier (v, cp1, cp2) -> FSketch.Bezier (evalVector v, evalVector cp1, evalVector cp2)

        let evalSubPath (subPath:SubPath) : FSketch.SubPath =
            {
                Start = evalVector subPath.Start
                Parts = List.map evalPathPart subPath.Parts
                Closed = subPath.Closed
            }

        let evalPath (path:Path) : FSketch.Path =
            { SubPaths = List.map evalSubPath path.SubPaths }

        let evalArgbColor (color:ArgbColor) : FSketch.ArgbColor = {
            Alpha = eval color.Alpha
            R = eval color.R
            G = eval color.G
            B = eval color.B }

        let evalHslaColor (color:HslaColor) : FSketch.HslaColor = {
            H = eval color.H
            S = eval color.S
            L = eval color.L
            Alpha = eval color.Alpha }

        let evalColor (color:Color) : FSketch.Color =
            match color with
            | ArgbColor color -> FSketch.ArgbColor (evalArgbColor color)
            | HslaColor color -> FSketch.HslaColor (evalHslaColor color)

        let evalLineJoin (lineJoin:LineJoin) : FSketch.LineJoin =
            match lineJoin with
            | LineJoin.Miter -> FSketch.LineJoin.Miter
            | LineJoin.Round -> FSketch.LineJoin.Round

        let evalPen (pen:Pen) : FSketch.Pen = {
            Color = evalColor pen.Color
            Thickness = eval pen.Thickness
            LineJoin = evalLineJoin pen.LineJoin }

        let evalBrush (brush:Brush) : FSketch.Brush =
            match brush with
            | SolidBrush color -> FSketch.SolidBrush (evalColor color)

        let evalDrawType (drawType:DrawType) : FSketch.DrawType =
            match drawType with
            | Contour pen -> FSketch.Contour (evalPen pen)
            | Fill brush -> FSketch.Fill (evalBrush brush)
            | ContourAndFill (pen, brush) -> FSketch.ContourAndFill(evalPen pen, evalBrush brush)

        let evalFont (font:Font) : FSketch.Font =
            match font with
            | Font.Arial -> FSketch.Font.Arial
            | Font.UnclosedSinglePathFont fontName -> FSketch.Font.UnclosedSinglePathFont fontName

        let evalHorizontalAlign (align:HorizontalAlign) : FSketch.HorizontalAlign =
            match align with
            | Left -> FSketch.HorizontalAlign.Left
            | Center -> FSketch.HorizontalAlign.Center
            | Right -> FSketch.HorizontalAlign.Right

        let evalVerticalAlign (align:VerticalAlign) : FSketch.VerticalAlign =
            match align with
            | Top -> FSketch.VerticalAlign.Top
            | Middle -> FSketch.VerticalAlign.Middle
            | Bottom -> FSketch.VerticalAlign.Bottom

        let evalText (text:Text) : FSketch.Text = {
            Text = text.Text
            Size = eval text.Size
            Font = evalFont text.Font
            HorizontalAlign = evalHorizontalAlign text.HorizontalAlign
            VerticalAlign = evalVerticalAlign text.VerticalAlign }

        let evalShape (shape:Shape) : FSketch.Shape =
            match shape with
            | Rectangle size ->
                FSketch.Shape.Rectangle(evalVector size)
            | Ellipse size ->
                FSketch.Shape.Ellipse(evalVector size)
            | Path (path) ->
                FSketch.Shape.Path(evalPath path)
            | Text (text) ->
                FSketch.Shape.Text(evalText text)

        let evalStyledShape (styledShape:StyledShape) : FSketch.StyledShape =
            {
                Shape = evalShape styledShape.Shape
                DrawType = evalDrawType styledShape.DrawType
            }

        let evalPlacedShape (refSpace:RefSpace, styledShape:StyledShape) =
            evalRefSpace refSpace, evalStyledShape styledShape

        let evalViewport (viewport:Viewport) : FSketch.Viewport =
            {
                Center = evalVector viewport.Center
                ViewSize = evalVector viewport.ViewSize
            }

        {
            Shapes = shapes |> List.map evalPlacedShape
            Viewport = viewport |> Option.map evalViewport
        }

    let atTime time (shapes:Shapes, viewport: Viewport option) =
        let context = { Time = time }
        let eval (Behaviour(f)) = f context
        (shapes, viewport) |> takeSnapshot eval

    let toFrames (frameDuration:float) (scene:Scene) =
        let framesCount = (scene.Duration / frameDuration) |> floor |> int
        seq {
            for index in 0 .. framesCount - 1 do
            let time = float index * frameDuration |> scene.TimeTransform
            yield (scene.Shapes, scene.Viewport) |> atTime time
        }

    let record (frameRate:int) scenes : FSketch.RenderedScene =
        let frameDuration = 1. / float frameRate
        {
            FrameDuration = frameDuration
            Frames = scenes |> Seq.collect (toFrames frameDuration) |> Seq.toArray
        }
