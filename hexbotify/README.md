## Hexbotify API
![hexbotify](https://i.imgur.com/CJdeIcW.png)

Retrieves an image and overlays it with the colors and coordinates returned by the [hexbot API](https://api.noopschallenge.com/hexbot).

The API exposes a single endpoint: `GET /api/hexbotify`

### Request Parameters (all optional):
| Parameters | Description |
|------------|-------------|
| canvas (string) | URL to an image to be used as the canvas. If not supplied, or if the URL does not exist, or if it is invalid (i.e.: not an image), the API will use a default canvas with black background instead.<br /><br />**NOTE**: The value needs to be URL-encoded.<br /><br />**NOTE**: If the default canvas is being used and either the width or the height parameter is not supplied, the API will default the canvas width to 800 or the canvas height to 600. |
| count (numeric) | Sent to the hexbot API as the count parameter. If not supplied, or if it is invalid (i.e.: less than 1), the API defaults it to 1/3 the resolution of the canvas. |
| width (numeric) | Sent to the hexbot API as the width parameter. If the value is greater than the width of the canvas, or if it is not supplied, or if it is invalid (i.e.: less than 1), the width of the canvas is used instead.<br /><br />**NOTE**: If the default canvas is being used, the width value also dictates the width of the canvas. |
| height (numeric) | Sent to the hexbot API as the height parameter. If the value is greater than the height of the canvas, or if it is not supplied, or if it is invalid (i.e.: less than 1), the height of the canvas is used instead.<br /><br />**NOTE**: If the default canvas is being used, the height value also dictates the height of the canvas. |
| seed (string) | Sent to the hexbot API as the seed parameter. |

More info on the hexbot API can be found [here](https://github.com/noops-challenge/hexbot).

### Sample

**Original image (for reference)**

`GET https://i.imgur.com/lnaGvzZ.jpg`

![original](https://i.imgur.com/lnaGvzZ.jpg)

**Hexbotified image**

`GET /api/hexbotify?canvas=https%3A%2F%2Fi.imgur.com%2FlnaGvzZ.jpg`

![hexbotified](https://i.imgur.com/x7ffazF.png)

**Hexbotified image (with width and height parameters)**

`GET /api/hexbotify?width=100&height=100&canvas=https%3A%2F%2Fi.imgur.com%2FlnaGvzZ.jpg`

![hexbotified with width and height](https://i.imgur.com/T9oeaE6.png)

**Hexbotified image (with count, width, and height parameters)**

`GET /api/hexbotify?count=100&width=100&height=100&canvas=https%3A%2F%2Fi.imgur.com%2FlnaGvzZ.jpg`

![hexbotified with count, width, and height](https://i.imgur.com/JBdz5Gp.png)

**Hexbotified image (default canvas)**

`GET /api/hexbotify`

![hexbotified with default canvas](https://i.imgur.com/TYNuDuq.png)