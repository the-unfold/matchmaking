# Troubleshooting

## `docker-compose build` кричит про `no space left on device`

Тупое и надёжное решение - удалить все тома, образы, слои и билд-кэш:

```sh
docker volume prune
docker system prune -a
```

## Elmish нигде не показывает ошибки

Надо помнить, что для Commands очень желательно передать обработчик ошибок, который даст возможность их обработать.