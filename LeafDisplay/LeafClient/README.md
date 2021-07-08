# Build

## Docker

### Build Image
```
docker build -t image-leaf-client .
```

### Test Image
```
docker run -it --rm image-leaf-client
```

### Login To GitHub Container Registry
```
echo $GHDockerToken | docker login ghcr.io -u hagronnestad --password-stdin
```

### Push Image To GitHub Container Registry
```
docker tag image-leaf-client ghcr.io/hagronnestad/image-leaf-client
docker push ghcr.io/hagronnestad/image-leaf-client:latest
```


# Run

### Login To GitHub Container Registry
```
echo $GHDockerToken | docker login ghcr.io -u hagronnestad --password-stdin
```

### Pull Image From GitHub Container Registry
```
docker pull ghcr.io/hagronnestad/image-leaf-client:latest
docker run -it --rm ghcr.io/hagronnestad/image-leaf-client
```


# Usage

```
Usage: leafclient username password [-o {filename}] [-p {url}] [-last]

Options:
        username        Your Nissan Connect username.
        password        Your blowfish encrypted password.
                        Encrypt your password at http://sladex.org/blowfish.js/.
                        Key: 'uyI5Dj9g8VCOFDnBRUbr3g'. Cipher mode: ECB. Output type: BASE64.

        -o              Outputs the result as JSON to {filename}.
        -p              Posts the result as JSON to {url}. Use comma for multiple urls.
        -last           Don't query live data from car.
```