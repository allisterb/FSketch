﻿namespace FSketch

module Dsl =

    let rectangle (width, height) = ClosedShape.Rectangle(Vector(width, height))
    let square (width) = ClosedShape.Rectangle(Vector(width, width))
    let ellipse (width, height) = ClosedShape.Ellipse(Vector(width, height))
    let circle (radius) = ClosedShape.Ellipse(Vector(radius * 2.0, radius * 2.0))
    let line (x1, y1) (x2, y2) = RefSpace.At(x1, y1), Line(Vector(x2 - x1, y2 - y1))
    let bezier (x1, y1) (x2, y2) (tx1, ty1) (tx2, ty2) = RefSpace.At(x1, y1), Bezier(Vector(x2 - x1, y2 - y1), Vector(tx1, ty1), Vector(tx2, ty2))
    let lineTo (x, y) = Line(Vector(x, y))
    let bezierTo (x, y) (tx1, ty1) (tx2, ty2) = Bezier(Vector(x, y), Vector(tx1, ty1), Vector(tx2, ty2))
    let toPath = CompositePath
    let toClosedPath = CompositePath >> ClosedPath
    let text format = Printf.ksprintf (fun s -> { Text = s; Size = 10. }) format
    let withSize size text = { text with Size = size }

    let withContour pen (space, shape) = space, ClosedShape(shape, Contour(pen))
    let withFill brush (space, shape) = space, ClosedShape(shape, Fill(brush))
    let withContourAndFill (pen, brush) (space, shape) = space, ClosedShape(shape, ContourAndFill(pen, brush))
    let withPen pen (space, path) = space, Path(path, pen)
    let writtenWith brush (space, text) = space, Text(text, brush)

    let transform matrix refSpace = RefSpace.Transform(matrix) + refSpace

    let at (x, y) element = RefSpace.At(x, y), element
    let withZ z (refSpace, element) = { refSpace with z = z }, element
    let translatedBy (x, y) (refSpace:RefSpace, element) = ({refSpace with transform = (Transforms.translate (x, y)) * refSpace.transform}, element)
    let rotatedBy alpha (refSpace:RefSpace, element) = ({refSpace with transform = (Transforms.rotate alpha) * refSpace.transform}, element)
    let scaledBy ratio (refSpace:RefSpace, element) = ({refSpace with transform = (Transforms.scale ratio) * refSpace.transform}, element)
    let scaledByX ratio (refSpace:RefSpace, element) = ({refSpace with transform = (Transforms.scaleX ratio) * refSpace.transform}, element)
    let scaledByY ratio (refSpace:RefSpace, element) = ({refSpace with transform = (Transforms.scaleY ratio) * refSpace.transform}, element)
    let xFlipped (refSpace:RefSpace, element) = ({refSpace with transform = (Transforms.scaleX -1.0) * refSpace.transform}, element)
    let yFlipped (refSpace:RefSpace, element) = ({refSpace with transform = (Transforms.scaleY -1.0) * refSpace.transform}, element)
    let origin = (0.0, 0.0)

    let placedMap f (r, s) = r, f(s)

    let Pi = System.Math.PI
