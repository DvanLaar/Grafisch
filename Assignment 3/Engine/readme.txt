Graphics Assignment 3
door
Daan van Laar   - 5518741
Ludo Pulles     - 5727146
Jasper Everink  - 5649137

Note:

Besturing:
	Left-Shift, Right-Shift			- versnelt beweging

	P								- Zet Normalmapping uit
	O								- Zet Normalmapping aan
	 
	Z								- Verandert laatste postprocessing kernel naar SmallGaussian
	X								- Verandert laatste postprocessing kernel naar HorizontalSobel (Horizontaal deel van de Sobel operator voor edge detection)

	Tab								- Creeert een nieuw lichtpunt in (0,0,0) met intensiteit (100,100,100)
	Delete							- Verwijdert geselecteerde lichtpunt
	Comma							- Selecteert volgend lichtpunt
	Period							- Selecteert vorig lichtpunt

	L, H							- Transleert geselecteerde lichtpunt over X-as
	N, M							- Transleert geselecteerde lichtpunt over Y-as
	J, K							- Transleert geselecteerde lichtpunt over Z-as

	Up, Down, Left, Right			- Roteert camera
	D, A							- Transleert camera over X-as
	E, Q							- Transleert camera over Y-as
	S, W							- Transleert camera over Z-as

	1, 2							- Transleert de stapel teapots over de relatieve y-as
	3, 4							- Roteert de stapel teapots rond the X-as

Main Assigments:
	Camera:
		 De camera is geimplementeerd met een tranlatie vector en een quaternion, die gebruikt worden om de camera matrix the updaten nadat er een translatie of rotatie op is toegepast. 
	Scene graph:
		De scene graph bestaat uit knopen met ieder een lokale transformatie matrix, kind knopen en kind models.
		Een main render methode wordt aangeroepen die recursief door de boom gaat, de lokate transformatie matrix met de modelToWorld matrix vermenigvuldigt, ieder kind knoop rendert met de aangepaste modelToWorld matrix
		en alle kind models rendert.
		Een model bevat extra data die nodig is voor het renderen van een mesh (bijvoorbeeld textures, kleur en shader).
	Shaders:
		De Phong shading model is zowel in fs.glsl als fs_normal.glsl geimplementeerd (fs_normal is met normal mapping, fs zonder).
		Hiervoor wordt eerst het ambient deel bepaalt, waarna voor iedere lichtbron in de scene het toegevoegde diffuse en specular deel wordt toegevoegt.
	Demo:
		De demo bestaat uit een vloer, verschillende teapots, een heightmap met The Trump erop, een aantal lichten en een skybox.
		De onderste vloer en teapot zijn voorzien van een normalmap. (Kan met O en P aan en uit gezet worden) 
		De teapot erboven heeft een environment map. Die daarboven is wat gescalet en de bovenste is geroteerd en heeft simpele shortfur.
		Met de 1, 2, 3 en 4 kunnen de drie bovenste teapots bewogen worden. (Om de werking van de scenegraph transformaties te laten zien)
		Het effect van HDR is te zien aan overvelle belichting van sommige lichten (en van de skybox)
		Er is vignetting en chromatic aberration te zien aan de randen van het scherm.
		De laatste kernel voor postprocessing (voor bijvoorbeeld blurren) kan met Z en X verandert worden tussen een kleine Gaussian Blur en voor iets artistieks naar het horizontaal deel van de Sobel operator (voor een edge detection look).
		Er kunnen lichtbronnen worden toegevoegt, verwijdert en getransleert. (Zie daarvoor de besturing hierboven)
	Documentation:
		Je leest het nu...

Bonus Assigments:
	Multiple lights:
		Er kunnen meerdere lichten bestuurd worden (en toegevoegt en verwijdert) tot een limiet van 100. Om niet te veel informatie naar de shader te moeten sturen
		is ervoor gekozen dat de diffuse en specular intensiteit van een lichtbron gelijk zijn. In de shader wordt eerst de ambient color aangebracht, waarna per licht de diffuse en specular delen worden toegevoegt.
	Cube mapping:
		6 Textures worden ingeladen in een cubemap texture. Deze cubemap wordt zowel gebruikt om een skybox te creeeren als voor de statische reflectieve environment map. (Zichtbaar op een van de teapots)
	Normal mapping:
		Op sommige objecten zit een normal map. Dit is een texture die de richting van de normal in tangent space opgeslagen heeft als een color. Tangent space is een basis opgebouwd uit tangent, bitangent en normal. Als een pixel paarsig is, dan zijn de coordinaten dus (0, 0, 1) wat betekent dat hij in de oude richting van de normaal wijst. Met de normal map kunnen dus afwijking van de standaard normaal aangegeven worden. Hierdoor krijgt het plaatje meer diepte. Deze eigenschappen worden door een normal shader geregeld en bij het inladen van het model wordt de tangent berekend, waaruit de bitangent in de shader te berekenen is.
	Vignetting and Chromatic Aberration:
		De vignetting en chromatic aberration worden toegepast in dezelfde shader. Voor vignetting wordt de kleur vermenigvuldigt met de cosinus van de afstand tot een gegeven punt. (Waardoor het donkerder wordt naarmate deze afstand toeneemt)
		Voor de chromatic aberration worden de RGB waarden gelezen met kleine afwijkingen over de lijn naar een gegeven punt.
	Seperable Box Filter:
		Er worden twee vectoren naar een speciale shader gestuurd die de convolutie van deze twee vectoren over elke pixel uitvoert.
		De seperable box filter wordt gebruikt voor verschillende blurs en voor artistieke edge detection.
	HDR glow/bloom:
		De rendertarget is aangepast om meerdere textures te kunnen bevatten. Alle fragment shaders die gebruikt worden voor het renderen van de scene renderen naar een gewone buffer (met de gewone scene) en naar de HDRbuffer (waar de deel van de te hoge kleuren worden opgeslagen).
		De HDRbuffer wordt daarna geblurt met een box filter, waarna deze met de gewone scene wordt opgetelt. 
	Short fur:
		Een shortfur model rendert eerst de gewone mesh, waarna er meerdere lagen (32) met een fur texture worden gerendert op een mesh, waar iedere vertex een beetje is verplaatst in de richting van de normaal.
		Dit geeft het effect alsof er korte haartjes op het object zitten. (Tenzij je er heel erg op in gaat zoomen, dan ziet het er vreemd uit)

References:
https://learnopengl.com/ (gebruikt voor onder andere normal mapping, skybox en reflectiemapping)
http://www.opengl-tutorial.org/intermediate-tutorials/tutorial-13-normal-mapping/ (gebruikt voor normal mapping)
http://neokabuto.blogspot.nl/2013/03/opentk-tutorial-2-drawing-triangle.html (shader loader)
De slides van de colleges
