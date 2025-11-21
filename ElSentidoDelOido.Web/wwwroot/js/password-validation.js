// Validación cliente para contraseñas (integra con el formulario .form-custom)
// Requiere jQuery cargado.
(function ($) {
    $(function () {
        var config = {
            minlength: 8,
            requireUppercase: true,
            requireLowercase: true,
            requireDigit: true,
            requireSpecial: true
        };

        function checkPasswordRules(pwd) {
            if (!pwd) return { ok: false, message: "La contraseña es obligatoria." };
            if (pwd.length < config.minlength) return { ok: false, message: "La contraseña debe tener al menos " + config.minlength + " caracteres." };
            if (config.requireUppercase && !/[A-Z]/.test(pwd)) return { ok: false, message: "La contraseña debe contener al menos una letra mayúscula." };
            if (config.requireLowercase && !/[a-z]/.test(pwd)) return { ok: false, message: "La contraseña debe contener al menos una letra minúscula." };
            if (config.requireDigit && !/[0-9]/.test(pwd)) return { ok: false, message: "La contraseña debe contener al menos un número." };
            if (config.requireSpecial && !/[^a-zA-Z0-9]/.test(pwd)) return { ok: false, message: "La contraseña debe contener al menos un carácter especial." };
            if (/\s/.test(pwd)) return { ok: false, message: "La contraseña no puede contener espacios." };
            return { ok: true, message: "" };
        }

        // Lugar donde mostramos mensaje junto al control
        function showFieldError($input, message) {
            // eliminar mensajes previos
            $input.next(".pwd-error").remove();
            if (message) {
                var $err = $("<div class='text-danger pwd-error small mt-1'></div>").text(message);
                $input.after($err);
            }
        }

        // Añadir comprobación al cambiar el campo contraseña y confirmación
        $(".form-custom").on("input change", "input[name='Password'], input[name='ConfirmPassword']", function () {
            var $form = $(this).closest(".form-custom");
            var pwd = $form.find("input[name='Password']").val() || "";
            var confirm = $form.find("input[name='ConfirmPassword']").val() || "";

            var res = checkPasswordRules(pwd);
            showFieldError($form.find("input[name='Password']"), res.ok ? "" : res.message);

            // validación de confirm
            $form.find("input[name='ConfirmPassword']").next(".pwd-error").remove();
            if (confirm && pwd !== confirm) {
                showFieldError($form.find("input[name='ConfirmPassword']"), "Las contraseñas no coinciden.");
            }
        });

        // Validar en submit
        $(".form-custom").on("submit", function (e) {
            var $form = $(this);
            var pwd = $form.find("input[name='Password']").val() || "";
            var confirm = $form.find("input[name='ConfirmPassword']").val() || "";

            var res = checkPasswordRules(pwd);
            // limpiar errores previos
            $form.find(".pwd-error").remove();
            $form.find(".client-validation-summary").remove();

            var hasError = false;
            if (!res.ok) {
                hasError = true;
                showFieldError($form.find("input[name='Password']"), res.message);
            }

            if (confirm !== pwd) {
                hasError = true;
                showFieldError($form.find("input[name='ConfirmPassword']"), "Las contraseñas no coinciden.");
            }

            if (hasError) {
                // añadir summary al inicio del form (para integrarlo visualmente)
                var $summary = $("<div class='alert alert-danger client-validation-summary mt-2' role='alert'></div>");
                var msgs = [];
                if (!res.ok) msgs.push(res.message);
                if (confirm !== pwd) msgs.push("Las contraseñas no coinciden.");
                $summary.html(msgs.join("<br/>"));
                $form.prepend($summary);

                e.preventDefault();
                return false;
            }

            // permitir submit; servidor también validará.
            return true;
        });
    });
})(jQuery);