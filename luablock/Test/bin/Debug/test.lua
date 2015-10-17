
function isten()
	if vara == 10 then

		print("@a", "Vara is: ", vara)

	end
end

function istwenty()
	if vara == 20 then

		print("@a", "Vara is: ", vara)

	end
end

function main()

	vara = 10

	print("@a", "The vlaue is: ", vara)

	vara = vara + 50 + 15

	print("@a", "The vlaue is: ", vara)

	vara = vara - 70

	print("@a", "The vlaue is: ", vara)

	vara = vara * 2

	print("@a", "The vlaue is: ", vara)

	isten()
	
	vara = vara * 2
	
	print("@a", "The vlaue is: ", vara)
	
	istwenty()
end

main()