xof 0302txt 0064
Mesh skybox {
//VertexBuffer
//A box has 24 textured vertices.
//We order the vertices by faces and surround any face clockwise starting at its left upper corner.
//Center of the coordinate system = center of the box = Vector3.Zero.
//Left-handed coordinate system = Y-axis upwards = Vector3.Up.
24;
-1.0;  1.0; -1.0;, //front  left  up
 1.0;  1.0; -1.0;, //       right up
 1.0; -1.0; -1.0;, //       right down
-1.0; -1.0; -1.0;, //       left  down
 1.0;  1.0;  1.0;, //back   right down
-1.0;  1.0;  1.0;, //       left  down
-1.0; -1.0;  1.0;, //       left  up
 1.0; -1.0;  1.0;, //       right up
-1.0;  1.0;  1.0;, //top    left  up
 1.0;  1.0;  1.0;, //       right up
 1.0;  1.0; -1.0;, //       right down
-1.0;  1.0; -1.0;, //       left  down
-1.0; -1.0; -1.0;, //bottom left  up
 1.0; -1.0; -1.0;, //       right up
 1.0; -1.0;  1.0;, //       right down
-1.0; -1.0;  1.0;, //       left  down
 1.0;  1.0; -1.0;, //right  left  up
 1.0;  1.0;  1.0;, //       right up
 1.0; -1.0;  1.0;, //       right down
 1.0; -1.0; -1.0;, //       left  down
-1.0;  1.0;  1.0;, //left   left  up
-1.0;  1.0; -1.0;, //       right up
-1.0; -1.0; -1.0;, //       right down
-1.0; -1.0;  1.0;; //       left  down
//IndexBuffer
6;                 // 6 faces
4; 0, 1, 2, 3;,
4; 4, 5, 6, 7;,
4; 8, 9,10,11;,
4;12,13,14,15;,
4;16,17,18,19;,
4;20,21,22,23;;

MeshMaterialList { //just one 1024x1024 folding-carton-texture
1; 1; 0;;
Material {
1.0; 1.0; 1.0; 1.0;; // R = 1.0, G = 1.0, B = 1.0
0.0;
0.0; 0.0; 0.0;;
0.0; 0.0; 0.0;;
TextureFilename { "skybox_texture.jpg"; }
} //end of Material
} //end of MaterialList

MeshTextureCoords {
24; //All coordinates are shifted by +/- 0.001
    //otherwise rounding errors cause white edges
0.251; 0.501;  //front  left  up
0.499; 0.501;  //       right up
0.499; 0.749;  //       right down
0.251; 0.749;  //       left  down
0.499; 0.249;  //back   right down
0.251; 0.249;  //       left  down
0.251; 0.001;  //       left  up
0.499; 0.001;  //       right up
0.251; 0.251;  //top    left  up
0.499; 0.251;  //       right up
0.499; 0.499;  //       right down
0.251; 0.499;  //       left  down
0.251; 0.751;  //bottom left  up
0.499; 0.751;  //       right up
0.499; 0.998;  //       right down
0.251; 0.998;  //       left  down
0.501; 0.501;  //right  left  up
0.749; 0.501;  //       right up
0.749; 0.749;  //       right down
0.501; 0.749;  //       left  down
0.001; 0.501;  //left   left  up
0.249; 0.501;  //       right up
0.249; 0.749;  //       right down
0.001; 0.749;; //       left  down
}
} //end of Mesh skybox {