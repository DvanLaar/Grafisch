Graphics Assignment 2
door
Daan van Laar   - 5518741
Ludo Pulles     - 5727146
Jasper Everink  - 5649137

Besturing:
    W, A, S, D, Q, E            -   Beweegt de camera
    Pijltjes toetesen           -   Draait de camera
    R, F                        -   Verhoogt/Verlaagt Anti-Aliasing
    1 - 8, -, +                 -   Past Speed up (clustering van pixels) aan
    PageUp, PageDown            -   Verhoogt/Verlaagt FOV

    Linker muisknop + bewegen   -   Draait de camera
    Rechter muisknop            -   Centreert de camera op het geselecteerde punt
    
Primitieven:
    Plane, TexturedPlane
    Sphere, TexturedSphere
    Triangle, TexturedTriangle, NormalTriangle (untextured met normal interpolation)
    Quad, TexturedQuad
    Mesh (ingeladen vanuit een .obj bestand is een verzameling van NormalTriangle met een AABB eromheen voor snelheid)
    
Lichtsoorten:
    Light               -   Ambient Light
    DirectionalLight    -   Onafhankelijk van afstand, maar wel afhankelijk van richting
    PointLight          -   Afhankelijk van afstand en hoek van de normaal met de richting van het lichtpunt
    Spotlight           -   Zelfde als PointLight, maar dan alleen binnen een bepaalde hoek rond de richting van het licht
    AreaLight           -   Zelfde als PointLight, maar nu gemiddeld over een aantal willekeurige punten in de bijbehorende driehoek
    
Bonus Assigments (uit het lijstje):
    Triangle Support & Normal Interpolation
        Er zijn triangles toegevoegd, zowel untextured, untextured met normal interpolation en textured.
    Spotlights
        Bijna hetzelfde als de PointLights, maar met een richting en een maximale hoek.
        Door het inproduct van de genormalizeerde richting van de lichtbron naar een punt en de richting van de spotlight te vergelijken met de hoek
        wordt bepaaldt of de spotlight wel of geen impact heeft op dat punt. 
    Anti-Aliasing
        Voor iedere pixel wordt er gemiddeld over over meerdere rays verspreidt over een grid tussen zichzelf en de opvolgende pixels
    Textures to all primitives
        Er zijn getexturede varianten van iedere primitieve (op Mesh en normaltriangle na).
        Voor een plane worden twee basis vectoren bepaald, waar een punt op geprojecteerd wordt (modulo texture grote) voor de texture coordinaten.
        De Triangle en Quad werken doormiddel van Barycentrische coordinaten.
        De Sphere wordt getextured door de bolcoordinaten te mappen naar een cylinder, die vervolgens naar de texture gemapt wordt.
    Textured Skydome
        Als een ray geen primitive raakt (en niet gestopt wordt door de maximale recursiediepte), wordt de richting van de ray gebruikt
        om op dezelfde manier als een texturedsphere een texturecoordinate te bepalen.
    Stochastic Sampling of AreaLight
        Een AreaLight krijgt een Triangle mee. Om de belichting op een punt van een AreaLight te bepalen wordt er gemiddeld
        over meerdere willekeure samples op de Triangle.
        
Bonus Assigments (anders):
    Light en DirectionalLight
        Light geeft een kleur onafhankelijk van de plaats, normaal en mogelijke obstructies van een te belichten punt en geeft dus een standaard belichting.
        DirectionalLight geeft een kleur alleen afhankelijk van de normaal en de mogelijke obstructies van een te belichten punt
    .OBJ files
        Om uitgebreidere Meshes te kunnen gebruiken is er in Mesh een relatief simpele .OBJ file reader aanwezig.
        (Alleen simpele .OBJ files met vertices, vertexnormals en triangle faces)
    Quads
        Bijna gelijk aan twee triangles met gelijke normaal, maar makkelijker een afbeelding op te texturen.
	Axis-aligned bounding box
		Dit is gedaan om sneller intersecties af te kunnen strepen als een Ray er niet doorheengaat.
	Specularity
		Er zijn voor sommige objecten wat reflecties toegevoegd van wit licht wat dan wordt weerspiegelt ook op niet-spiegel oppervlakten. Hierbij gaat het meeste licht in de reflectie richting. Daarbuiten neemt het exponentieel af met exponent 'hardness'

References:
    -   De slides van het vak
    -   Moeller-Trumbore intersection algorithm as explained in:
        http://www.cs.virginia.edu/~gfx/Courses/2003/ImageSynthesis/papers/Acceleration/Fast%20MinimumStorage%20RayTriangle%20Intersection.pdf


